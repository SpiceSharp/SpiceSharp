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
        /// <param name="simulations">The task simulations to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(SubcircuitSimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<IBiasingSimulationState>();
            if (parameters.TryGetValue<BiasingParameters>(out var bp) &&
                (bp.ParallelLoad || bp.ParallelSolve) &&
                simulations.Length > 1)
            {
                if (bp.ParallelSolve)
                {
                    foreach (var sim in simulations)
                        sim.States.Add<IBiasingSimulationState>(new SolveBiasingState(state, parameters));
                }
                else
                {
                    foreach (var sim in simulations)
                        sim.States.Add<IBiasingSimulationState>(new LoadBiasingState(state, parameters));
                }
            }
            else
            {
                foreach (var sim in simulations)
                    sim.States.Add(state);
            }
        }
    }
}
