using System;
using System.Collections;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Contains and manages a collection circuit objects.
    /// </summary>
    public class CircuitObjects : IEnumerable<ICircuitObject>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<CircuitIdentifier, ICircuitObject> objects = new Dictionary<CircuitIdentifier, ICircuitObject>();
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
        /// Search for an object by path
        /// </summary>
        /// <param name="path">The path of the object</param>
        /// <returns></returns>
        public ICircuitObject this[string name] => objects[name];
        
        /// <summary>
        /// The amount of circuit objects
        /// </summary>
        public int Count => objects.Count;

        /// <summary>
        /// Clear all circuit objects
        /// </summary>
        public void Clear()
        {
            objects.Clear();
            ordered.Clear();
            isordered = false;
        }

        /// <summary>
        /// Add one or more circuit objects
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
        /// Remove specific circuit objects from the collection
        /// </summary>
        /// <param name="names">Names of the objects that need to be deleted</param>
        public void Remove(params string[] names)
        {
            foreach (var name in names)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                objects.Remove(name);

                // Note: Removing objects does not interfere with the order!
            }
        }

        /// <summary>
        /// Check if a component exists
        /// Multiple names can be specified, in which case the first names will refer to subcircuits
        /// </summary>
        /// <param name="name">A list of names. If there are multiple names, the first names will refer to a subcircuit</param>
        /// <returns></returns>
        public bool Contains(string name) => objects.ContainsKey(name);

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
        public void BuildOrderedComponentList() => BuildOrderedComponentList(this);

        /// <summary>
        /// Build ordered list of components
        /// This method is called recursively for subcircuits
        /// </summary>
        /// <param name="root"></param>
        private void BuildOrderedComponentList(CircuitObjects root)
        {
            if (isordered)
                return;

            // Initialize
            ordered.Clear();
            HashSet<ICircuitObject> toadd = new HashSet<ICircuitObject>();

            // Build our list
            foreach (var c in objects.Values)
            {
                ordered.Add(c);

                // Keep track of the models that aren't part of the circuit yet
                if (c is ICircuitComponent component)
                {
                    var model = component.Model;
                    if (model != null)
                        toadd.Add(model);
                }
            }

            // Add models automatically to the root object list
            // This way, we are sure that subcircuits don't add them multiple times
            foreach (var model in toadd)
            {
                if (!ordered.Contains(model) && !root.ordered.Contains(model))
                    root.ordered.Add(model);
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
