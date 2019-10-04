using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An <see cref="IParallelPreparer"/> for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class FrequencyPreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Prepare(ISimulation simulation)
        {
        }

        /// <summary>
        /// Restores the specified simulation from parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Restore(ISimulation simulation)
        {
        }
    }
}
