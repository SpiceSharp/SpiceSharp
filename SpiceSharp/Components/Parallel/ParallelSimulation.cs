using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelComponents
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
        public IParameterSetCollection LocalParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSimulation"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parallel component parameters.</param>
        public ParallelSimulation(ISimulation parent, IParameterSetCollection parameters)
            : base(parent,
                  new BehaviorContainerCollection(parent?.EntityBehaviors.Comparer),
                  new InheritedTypeSet<ISimulationState>())
        {
            LocalParameters = parameters.ThrowIfNull(nameof(parameters));
        }

        /// <inheritdoc/>
        public override S GetState<S>()
        {
            if (LocalStates.TryGetValue(out S result))
                return result;
            return Parent.GetState<S>();
        }
    }
}
