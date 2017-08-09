using System;
using System.Collections;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// This class represents all components in the circuit
    /// </summary>
    public class CircuitComponents : IEnumerable<CircuitComponent>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, CircuitComponent> components = new Dictionary<string, CircuitComponent>();
        private List<CircuitComponent> ordered = new List<CircuitComponent>();

        /// <summary>
        /// Gets whether or not the list is already ordered
        /// </summary>
        private bool isordered = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitComponents() { }

        /// <summary>
        /// Search for a circuit component with a specific priority
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <returns></returns>
        public CircuitComponent this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (components.ContainsKey(name))
                    return components[name];
                throw new CircuitException($"Could not find {name}");
            }
        }

        /// <summary>
        /// Add one or more components
        /// </summary>
        /// <param name="cs">The components that need to be added</param>
        public void Add(params CircuitComponent[] cs)
        {
            foreach (var c in cs)
            {
                if (c == null)
                    throw new ArgumentNullException(nameof(c));
                if (components.ContainsKey(c.Name))
                    throw new CircuitException($"A component with the name {c.Name} already exists");
                components.Add(c.Name, c);
                isordered = false;
            }
        }

        /// <summary>
        /// Remove a component from a specific priority
        /// </summary>
        /// <param name="names">The names of the components that need to be deleted</param>
        public void Remove(params string[] names)
        {
            foreach (var name in names)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                if (components.ContainsKey(name))
                    components.Remove(name);
                isordered = false;
            }
        }

        /// <summary>
        /// Check if a component exists
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <returns></returns>
        public bool Contains(string name) => components.ContainsKey(name);

        /// <summary>
        /// Get all components of a specific type
        /// </summary>
        /// <param name="type">The type of components you wish to find</param>
        /// <returns></returns>
        public CircuitComponent[] ByType(Type type)
        {
            List<CircuitComponent> components = new List<CircuitComponent>();
            foreach (var c in this.components.Values)
            {
                if (c.GetType() == type)
                    components.Add(c);
            }
            return components.ToArray();
        }

        /// <summary>
        /// This method will generate a list of circuit components with first all models, followed by all the components
        /// </summary>
        public void BuildOrderedComponentList()
        {
            if (isordered)
                return;

            // Initialize
            ordered.Clear();
            var mods = new HashSet<CircuitModel>();

            // Build our list
            foreach (var c in components.Values)
            {
                // Add models only once
                var model = c.GetModel();
                if (model != null && !mods.Contains(model))
                {
                    mods.Add(model);
                    ordered.Add(model);
                }

                // Add the components
                ordered.Add(c);
            }

            // Sort the list based on priority
            ordered.Sort((CircuitComponent a, CircuitComponent b) => {
                return b.Priority.CompareTo(a.Priority);
            });
            isordered = true;
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CircuitComponent> GetEnumerator() => ordered.GetEnumerator();

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ordered.GetEnumerator();
    }
}
