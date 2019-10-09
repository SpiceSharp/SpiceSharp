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
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(IParallelSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<IBiasingSimulationState>();
            taskSimulation.States.Add<IBiasingSimulationState>(
                new BiasingSimulationState(
                    state, 
                    new LocalSolver<double>(state.Solver, taskSimulation.Task)
                    )
                );
        }
    }
}
