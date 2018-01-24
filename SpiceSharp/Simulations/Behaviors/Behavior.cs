using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Represents a behavior for a class
    /// </summary>
    public abstract class Behavior
    {
        /// <summary>
        /// The component the behavior acts upon
        /// </summary>
        protected Entity Component { get; private set; }
        
        /// <summary>
        /// Get the name of the behavior
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Gets whether or not the behavior is already set up
        /// </summary>
        public bool DataOnly { get; protected set; } = false;

        /// <summary>
        /// Constructor
        /// NOTE: remove default later
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public Behavior(Identifier name = null)
        {
            Name = name;
        }

        /// <summary>
        /// Get a behavior of a specific type
        /// </summary>
        /// <typeparam name="T">The circuit behavior we want to ask</typeparam>
        /// <param name="co">The circuit object with the behaviors</param>
        /// <returns></returns>
        protected T GetBehavior<T>(Entity co) where T : Behavior
        {
            // Get base class
            var behavior = co.GetBehavior(typeof(T).BaseType) as T;
            if (behavior == null)
                throw new CircuitException($"{co.Name}: Could not find behavior \"{typeof(T).Name}\"");
            return behavior;
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">The data provider</param>
        public virtual void Setup(SetupDataProvider provider)
        {
            // Do nothing
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public virtual void Setup(Entity component, Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public virtual void Unsetup()
        {
            // Do nothing
        }

        /// <summary>
        /// Create a delegate for extracting data
        /// </summary>
        /// <param name="property">Parameter</param>
        /// <returns>Returns null if there is no export method</returns>
        public virtual Func<State, double> CreateExport(string property)
        {
            // Find methods to create the export
            var members = GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var member in members)
            {
                // Check the return type (needs to be a double)
                if (member.ReturnType != typeof(double))
                    continue;

                // Check the name
                var names = (SpiceName[])member.GetCustomAttributes(typeof(SpiceName), true);
                bool found = false;
                foreach (var name in names)
                {
                    if (name.Name == property)
                    {
                        found = true;
                        continue;
                    }
                }
                if (!found)
                    continue;

                // Check the parameters
                var parameters = member.GetParameters();
                if (parameters.Length != 1)
                    continue;
                if (parameters[0].ParameterType != typeof(State))
                    continue;

                // Return a delegate
                return (Func<State, double>)member.CreateDelegate(typeof(Func<State, double>));
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Helper function for binding an extra equation in a circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="type">The type</param>
        /// <returns></returns>
        protected Node CreateNode(Circuit ckt, Identifier name, Node.NodeType type = Node.NodeType.Voltage)
        {
            // Map the extra equations
            return ckt.Nodes.Create(name, type);
        }
    }
}
