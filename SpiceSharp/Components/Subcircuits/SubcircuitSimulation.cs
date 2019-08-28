using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A simulation look-alike to use proxies.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.Simulation" />
    public class SubcircuitSimulation : Simulation
    {
        private Simulation _parent;
        private SubcircuitVariableSet _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the subcircuit simulation.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="types">The types needed by the parent simulation.</param>
        /// <param name="nodemap">The structure that maps the internal to external nodes.</param>
        public SubcircuitSimulation(string name, Simulation parent, IEnumerable<Type> types, Dictionary<string, string> nodemap)
            : base(name)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _variables = new SubcircuitVariableSet(name, nodemap, parent.Variables);

            // Copy all configurations from the parent simulation by reference
            foreach (var config in parent.Configurations)
                Configurations.Add(config);

            // Copy all states from the parent simulation by reference
            foreach (var state in parent.States)
                States.Add(state);

            // Store the types to be created
            foreach (var type in types)
                BehaviorTypes.Add(type);
        }

        /// <summary>
        /// Runs the simulation on the specified circuit.
        /// </summary>
        /// <param name="entities">The entities to simulate.</param>
        /// <exception cref="CircuitException">Cannot run subcircuit simulation</exception>
        public override void Run(EntityCollection entities)
        {
            // We're not going to execute a complete simulation, we just
            // want to create our behaviors and copy our
            Status = _parent.Status;
            Setup(entities);
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        /// <exception cref="CircuitException">Cannot execute subcircuit simulation</exception>
        protected override void Execute()
        {
            throw new CircuitException("Cannot execute subcircuit simulation");
        }

        /// <summary>
        /// Creates the variable set.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        protected override IVariableSet CreateVariableSet(EntityCollection entities) => _variables;
    }
}
