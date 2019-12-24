using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.Entities;
using SpiceSharp.General;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// A subcircuit simulation that captures created behaviors in a local container.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public class ParallelSimulation : SimulationWrapper
    {
        /// <summary>
        /// Gets the local configurations.
        /// </summary>
        /// <value>
        /// The local configurations.
        /// </value>
        public IParameterSetDictionary LocalConfigurations { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSimulation"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="configurations">The parallel configurations.</param>
        public ParallelSimulation(ISimulation parent, IParameterSetDictionary configurations)
            : base(parent, 
                  parent?.Configurations,
                  new BehaviorContainerCollection(parent?.EntityBehaviors.Comparer),
                  new InheritedTypeDictionary<ISimulationState>(),
                  parent?.Variables)
        {
            LocalConfigurations = configurations.ThrowIfNull(nameof(configurations));
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
