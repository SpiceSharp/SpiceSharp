using System;
using System.Collections.Generic;
using System.Text;
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
        private ISolver<double> _oldSolver;

        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Prepare(ISimulation simulation)
        {
            var state = simulation.States.GetValue<BiasingSimulationState>();
            _oldSolver = state.Solver;
            state.Solver = new LocalSolver<double>(_oldSolver);
        }

        /// <summary>
        /// Restores the specified simulation from parallel loading.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Restore(ISimulation simulation)
        {
            var state = simulation.States.GetValue<BiasingSimulationState>();
            state.Solver = _oldSolver;
        }
    }
}
