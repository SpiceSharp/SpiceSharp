using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Prepares <see cref="SubcircuitSimulation"/> for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    public static class FrequencyPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="simulations">The task simulations to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Prepare(SubcircuitSimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters)
        {
            if (parameters.TryGetValue<FrequencyParameters>(out var fp) &&
                (fp.ParallelLoad || fp.ParallelSolve) && simulations.Length > 1)
            {
                if (fp.ParallelSolve)
                {
                    foreach (var sim in simulations)
                    {
                        if (sim.Top is IStateful<IComplexSimulationState> t)
                            sim.States.Add<IComplexSimulationState>(new SolveComplexState(t.State));
                    }
                }
                else
                {
                    foreach (var sim in simulations)
                    {
                        if (sim.Top is IStateful<IComplexSimulationState> t)
                            sim.States.Add<IComplexSimulationState>(new LoadComplexState(t.State));
                    }
                }
            }
            else
            {
                foreach (var sim in simulations)
                {
                    if (sim.Top is IStateful<IComplexSimulationState> t)
                        sim.States.Add(t.State);
                }
            }
        }
    }
}
