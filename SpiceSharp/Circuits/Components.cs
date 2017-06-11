using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitComponents()
        {
        }

        /// <summary>
        /// Gets or sets a component by name
        /// </summary>
        /// <param name="name"></param>
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
        /// <param name="c"></param>
        public void Add(params CircuitComponent[] cs)
        {
            foreach (var c in cs)
            {
                if (c == null)
                    throw new ArgumentNullException(nameof(c));
                if (components.ContainsKey(c.Name))
                    throw new CircuitException($"A component with the name {c.Name} already exists");
                components.Add(c.Name, c);
            }
        }

        /// <summary>
        /// Remove a component
        /// </summary>
        /// <param name="name"></param>
        public void Remove(params string[] names)
        {
            foreach (var name in names)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (components.ContainsKey(name))
                    components.Remove(name);
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
        /// <param name="type"></param>
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
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CircuitComponent> GetEnumerator()
        {
            return components.Values.GetEnumerator();
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return components.Values.GetEnumerator();
        }
    }
}
