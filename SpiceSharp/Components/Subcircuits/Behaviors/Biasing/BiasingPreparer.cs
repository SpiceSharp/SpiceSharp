using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Prepares <see cref="SubcircuitSimulation"/> for <see cref="IBiasingBehavior"/>.
    /// </summary>
    public static class BiasingPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="simulations">The task simulations to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Prepare(SubcircuitSimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters)
        {
            if (parameters.TryGetValue<BiasingParameters>(out var bp) &&
                (bp.ParallelLoad || bp.ParallelSolve) &&
                simulations.Length > 1)
            {
                if (bp.ParallelSolve)
                {
                    foreach (var sim in simulations)
                    {
                        if (sim.Top is IStateful<IBiasingSimulationState> t)
                            sim.States.Add<IBiasingSimulationState>(new SolveBiasingState(t.State, bp));
                    }
                }
                else
                {
                    foreach (var sim in simulations)
                    {
                        if (sim.Top is IStateful<IBiasingSimulationState> t)
                        sim.States.Add<IBiasingSimulationState>(new LoadBiasingState(t.State));
                    }
                }
            }
            else
            {
                foreach (var sim in simulations)
                {
                    if (sim.Top is IStateful<IBiasingSimulationState> t)
                        sim.States.Add(t.State);
                }
            }
        }
    }
}
