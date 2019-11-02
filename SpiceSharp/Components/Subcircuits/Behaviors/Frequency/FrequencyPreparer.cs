using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="ISimulationPreparer"/> for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="ISimulationPreparer" />
    public class FrequencyPreparer : ISimulationPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="simulations">The task simulations to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(SubcircuitSimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<IComplexSimulationState>();
            if (parameters.TryGetValue<FrequencyParameters>(out var fp) &&
                (fp.ParallelLoad || fp.ParallelSolve) && simulations.Length > 1)
            {
                if (fp.ParallelSolve)
                {
                    foreach (var sim in simulations)
                        sim.States.Add<IComplexSimulationState>(new SolveComplexState(state));
                }
                else
                {
                    foreach (var sim in simulations)
                        sim.States.Add<IComplexSimulationState>(new LoadComplexState(state));
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
