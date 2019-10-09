using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A simulation that allows behaviors to be created for parallel computations.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public interface IParallelSimulation : ISimulation
    {
        /// <summary>
        /// Gets the parent simulation for which this one should be derived.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        ISimulation Parent { get; }

        /// <summary>
        /// Gets the task that the simulation will run with.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        int Task { get; }
    }
}
