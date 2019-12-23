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
        /// Initializes a new instance of the <see cref="ParallelSimulation"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation.</param>
        public ParallelSimulation(ISimulation parent)
            : base(parent, 
                  parent?.Configurations,
                  new BehaviorContainerCollection(parent?.EntityBehaviors.Comparer),
                  new InheritedTypeDictionary<ISimulationState>(),
                  parent?.Variables)
        {
        }
    }
}
