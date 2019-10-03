using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An <see cref="IParallelPreparer"/> for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class FrequencyPreparer : IParallelPreparer
    {
        private ISolver<Complex> _oldSolver;

        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Prepare(ISimulation simulation)
        {
            var state = simulation.States.GetValue<ComplexSimulationState>();
            _oldSolver = state.Solver;
            state.Solver = new LocalSolver<Complex>(_oldSolver);
        }

        /// <summary>
        /// Restores the specified simulation from parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Restore(ISimulation simulation)
        {
            var state = simulation.States.GetValue<ComplexSimulationState>();
            state.Solver = _oldSolver;
        }
    }
}
