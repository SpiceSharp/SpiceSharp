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
                if (c is Subcircuit)
                {
                    if (name.Length > 1)
                    {
                        string[] nn = new string[name.Length - 1];
                        for (int i = 1; i < name.Length; i++)
                            nn[i - 1] = name[i];
                        return (c as Subcircuit).Objects[nn];
                    }
                    else
                        throw new CircuitException($"Component \"{name[0]}\" does not exist");
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
                    objects.Remove(name);
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
        /// This method will generate a list of circuit objects with first all models, followed by all the objects
        /// </summary>
        public void BuildOrderedComponentList()
        {
            if (isordered)
                return;

            // Initialize
            ordered.Clear();
            var added = new HashSet<ICircuitObject>();

            // Build our list
            foreach (var c in objects.Values)
            {
                if (c is ICircuitComponent)
                {
                    var m = (c as ICircuitComponent).Model;
                    if (m != null && !added.Contains(m))
                    {
                        added.Add(m);
                        ordered.Add(m);
                    }
                }

                // Add the component if it is not already a model that was added
                if (!added.Contains(c))
                    ordered.Add(c);
            }

            // Sort the list based on priority
            ordered.Sort((ICircuitObject a, ICircuitObject b) => {
                return b.Priority.CompareTo(a.Priority);
            });
            isordered = true;
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
