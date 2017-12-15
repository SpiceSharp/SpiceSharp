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
    public class CircuitObjects : IEnumerable<CircuitObject>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<CircuitIdentifier, CircuitObject> objects = new Dictionary<CircuitIdentifier, CircuitObject>();
        private List<CircuitObject> ordered = new List<CircuitObject>();

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
        /// <param id="path">The path of the object</param>
        /// <returns></returns>
        public CircuitObject this[CircuitIdentifier id] => objects[id];
        
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
        /// <param id="cs">The objects that need to be added</param>
        public void Add(params CircuitObject[] cs)
        {
            foreach (var c in cs)
            {
                if (c == null)
                    throw new ArgumentNullException(nameof(c));
                if (objects.ContainsKey(c.Name))
                    throw new CircuitException($"A component with the id {c.Name} already exists");
                objects.Add(c.Name, c);
                isordered = false;
            }
        }

        /// <summary>
        /// Remove specific circuit objects from the collection
        /// </summary>
        /// <param id="names">Names of the objects that need to be deleted</param>
        public void Remove(params CircuitIdentifier[] ids)
        {
            foreach (var id in ids)
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));
                objects.Remove(id);

                // Note: Removing objects does not interfere with the order!
            }
        }

        /// <summary>
        /// Check if a component exists
        /// Multiple names can be specified, in which case the first names will refer to subcircuits
        /// </summary>
        /// <param id="id">A list of names. If there are multiple names, the first names will refer to a subcircuit</param>
        /// <returns></returns>
        public bool Contains(CircuitIdentifier id) => objects.ContainsKey(id);

        /// <summary>
        /// Get a circuit object
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <param id="obj"></param>
        /// <returns></returns>
        public bool TryGetObject(CircuitIdentifier id, out CircuitObject obj) => objects.TryGetValue(id, out obj);

        /// <summary>
        /// Get all objects of a specific type
        /// </summary>
        /// <param id="type">The type of objects you wish to find</param>
        /// <returns></returns>
        public CircuitObject[] ByType(Type type)
        {
            List<CircuitObject> result = new List<CircuitObject>();
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
            HashSet<CircuitObject> added = new HashSet<CircuitObject>();

            // Build our list
            foreach (var c in objects.Values)
            {
                // Add the object to the ordered list
                ordered.Add(c);
                added.Add(c);

                // Automatically add models to the ordered list
                if (c is Component component)
                {
                    var model = component.Model;
                    if (model != null && !added.Contains(model))
                    {
                        added.Add(model);
                        ordered.Add(model);
                    }
                }
            }

            // Sort the list based on priority
            ordered.Sort((CircuitObject a, CircuitObject b) => {
                return b.Priority.CompareTo(a.Priority);
            });
            isordered = true;
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CircuitObject> GetEnumerator() => ordered.GetEnumerator();

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ordered.GetEnumerator();
    }
}
