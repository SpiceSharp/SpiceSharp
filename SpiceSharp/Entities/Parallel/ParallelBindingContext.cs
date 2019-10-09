using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Binding context for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="BindingContext" />
    public class ParallelBindingContext : BindingContext
    {
        /// <summary>
        /// Gets the simulations.
        /// </summary>
        /// <value>
        /// The simulations.
        /// </value>
        public IParallelSimulation[] Simulations { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="simulations">The simulations used for each task.</param>
        public ParallelBindingContext(ISimulation simulation, BehaviorContainer behaviors, IParallelSimulation[] simulations)
            : base(simulation, behaviors)
        {
            Simulations = simulations.ThrowIfEmpty(nameof(simulations));
        }
    }
}
