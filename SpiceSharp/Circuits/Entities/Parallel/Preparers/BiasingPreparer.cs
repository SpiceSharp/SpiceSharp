using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An <see cref="IParallelPreparer"/> for the <see cref="IBiasingBehavior"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class BiasingPreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Prepare(ISimulation simulation)
        {
            var psim = (ParallelSimulation)simulation;
            var state = psim.Parent.States.GetValue<IBiasingSimulationState>();
            psim.States.Add<IBiasingSimulationState>(new BiasingSimulationState(state));
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
