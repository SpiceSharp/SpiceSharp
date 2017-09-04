using System;
using System.Collections;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// This class represents all objects in the circuit
    /// </summary>
    public class CircuitObjects : IEnumerable<ICircuitObject>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, ICircuitObject> objects = new Dictionary<string, ICircuitObject>();
        private HashSet<Subcircuit> subckts = new HashSet<Subcircuit>();
        private List<ICircuitObject> ordered = new List<ICircuitObject>();

        /// <summary>
        /// Gets whether or not the list is already ordered
        /// </summary>
        private bool isordered = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitObjects() { }

        /// <summary>
        /// Search for a circuit component with a specific priority
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <returns></returns>
        public ICircuitObject this[params string[] name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (name.Length == 0)
                    throw new ArgumentException("At least one name expected", nameof(name));

                if (!objects.ContainsKey(name[0]))
                    throw new CircuitException($"Component \"{name[0]}\" does not exist");

                ICircuitObject c = objects[name[0]];
                if (c is Subcircuit && name.Length > 1)
                {
                    string[] nn = new string[name.Length - 1];
                    for (int i = 1; i < name.Length; i++)
                        nn[i - 1] = name[i];
                    return (c as Subcircuit).Objects[nn];
                }
                else
                {
                    if (name.Length > 1)
                        throw new CircuitException($"Component \"{name[0]}\" is not a subcircuit");
                    return c;
                }
            }
        }

        /// <summary>
        /// The amount of objects
        /// </summary>
        public int Count => objects.Count;

        /// <summary>
        /// Clear all objects
        /// </summary>
        public void Clear()
        {
            objects.Clear();
            ordered.Clear();
            isordered = false;
        }

        /// <summary>
        /// Add one or more objects
        /// </summary>
        /// <param name="cs">The objects that need to be added</param>
        public void Add(params ICircuitObject[] cs)
        {
            foreach (var c in cs)
            {
                if (c == null)
                    throw new ArgumentNullException(nameof(c));
                if (objects.ContainsKey(c.Name))
                    throw new CircuitException($"A component with the name {c.Name} already exists");
                objects.Add(c.Name, c);
                if (c is Subcircuit)
                    subckts.Add((Subcircuit)c);
                isordered = false;
            }
        }

        /// <summary>
        /// Remove a component from a specific priority
        /// </summary>
        /// <param name="names">The names of the objects that need to be deleted</param>
        public void Remove(params string[] names)
        {
            foreach (var name in names)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                if (objects.ContainsKey(name))
                {
                    if (objects[name] is Subcircuit)
                        subckts.Remove((Subcircuit)objects[name]);
                    objects.Remove(name);
                }
                isordered = false;
            }
        }

         /// <summary>
        /// Check if a component exists
        /// Multiple names can be specified, in which case the first names will refer to subcircuits
        /// </summary>
        /// <param name="name">A list of names. If there are multiple names, the first names will refer to a subcircuit</param>
        /// <returns></returns>
        public bool Contains(params string[] name)
        {
            if (name.Length == 0)
                throw new ArgumentException("At least one name expected", nameof(name));

            if (!objects.ContainsKey(name[0]))
                return false;

            ICircuitObject c = objects[name[0]];
            if (c is Subcircuit)
            {
                if (name.Length > 1)
                {
                    string[] nn = new string[name.Length - 1];
                    for (int i = 1; i < name.Length; i++)
                        nn[i - 1] = name[i];
                    return (c as Subcircuit).Objects.Contains(nn);
                }
                else
                    return true;
            }
            else
            {
                if (name.Length > 1)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Get all objects of a specific type
        /// </summary>
        /// <param name="type">The type of objects you wish to find</param>
        /// <returns></returns>
        public ICircuitObject[] ByType(Type type)
        {
            List<ICircuitObject> result = new List<ICircuitObject>();
            foreach (var c in objects.Values)
            {
                if (c.GetType() == type)
                    result.Add(c);
            }
            return result.ToArray();
        }

        /// <summary>
        /// This method is called when building an ordered list of circuit objects
        /// Circuit objects will be called by descending priority
        /// </summary>
        public void BuildOrderedComponentList()
        {
            if (isordered)
                return;

            // Initialize
            ordered.Clear();

            // Build our list
            foreach (var c in objects.Values)
            {
                ordered.Add(c);

                // Do ordering for subcircuits
                if (c is Subcircuit)
                    (c as Subcircuit).Objects.BuildOrderedComponentList();
            }

            // Sort the list based on priority
            ordered.Sort((ICircuitObject a, ICircuitObject b) => {
                return b.Priority.CompareTo(a.Priority);
            });
            isordered = true;
        }

        /// <summary>
        /// Find the circuitobject and expand the path
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string[] FindPath(ICircuitObject o)
        {
            List<string> path = new List<string>();
            if (TryFindPath(o, path))
                return path.ToArray();
            return null;
        }

        /// <summary>
        /// Recursively search for a circuit object and return the path
        /// </summary>
        /// <param name="o">The object to search</param>
        /// <param name="path">The path</param>
        /// <returns></returns>
        protected bool TryFindPath(ICircuitObject o, List<string> path)
        {
            if (objects.ContainsKey(o.Name) && objects[o.Name] == o)
            {
                path.Add(o.Name);
                return true;
            }
            else
            {
                foreach (var subckt in subckts)
                {
                    if (subckt.Objects.TryFindPath(o, path))
                    {
                        path.Insert(0, subckt.Name);
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ICircuitObject> GetEnumerator() => ordered.GetEnumerator();

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ordered.GetEnumerator();
    }
}
