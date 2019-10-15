using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="ISimulationPreparer"/> for the <see cref="IBiasingBehavior"/>.
    /// </summary>
    /// <seealso cref="ISimulationPreparer" />
    public class BiasingPreparer : ISimulationPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(SubcircuitSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<IBiasingSimulationState>();
            if (parameters.TryGetValue<BiasingParameters>(out var bp) && 
                bp.ParallelLoad && taskSimulation.Tasks > 1)
            {
                // We need to prepare for multithreading
                taskSimulation.States.Add<IBiasingSimulationState>(
                    new BiasingSimulationState(
                        state,
                        new SolverElementProvider<double>(state.Solver)
                        )
                    );
            }
            else
                taskSimulation.States.Add(state);
        }
    }
}
