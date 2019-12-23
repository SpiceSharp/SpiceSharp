using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.General;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A subcircuit simulation that captures created behaviors in a local container.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public class SubcircuitSimulation : SimulationWrapper
    {
        /// <summary>
        /// Gets the local parameters.
        /// </summary>
        /// <value>
        /// The local parameters.
        /// </value>
        public IParameterSetDictionary LocalConfiguration { get; }

        /// <summary>
        /// Gets the variables that are shared between the subcircuit simulation and the parent simulation.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        public IEnumerable<Variable> SharedVariables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation" /> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="configuration">The configuration for the subcircuit.</param>
        /// <param name="shared">The shared variables.</param>
        public SubcircuitSimulation(string name, ISimulation parent, IParameterSetDictionary configuration, IEnumerable<Variable> shared)
            : base(parent,
                  parent?.Configurations,
                  new BehaviorContainerCollection(),
                  new InterfaceTypeDictionary<ISimulationState>(),
                  new SubcircuitVariableSet(name, parent?.Variables))
        {
            LocalConfiguration = configuration.ThrowIfNull(nameof(configuration));
            SharedVariables = shared.ThrowIfNull(nameof(shared));
        }

        /// <summary>
        /// Gets the state of the specified type.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// The state, or <c>null</c> if the state isn't used.
        /// </returns>
        public override S GetState<S>()
        {
            if (LocalStates.TryGetValue(out S result))
                return result;
            return Parent.GetState<S>();
        }
    }
}
