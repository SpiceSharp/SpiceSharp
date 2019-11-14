using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Prepares <see cref="SubcircuitSimulation"/> for <see cref="ITimeBehavior"/>.
    /// </summary>
    public static class TimePreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="simulations">The task simulations to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Prepare(SubcircuitSimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters)
        {
            foreach (var sim in simulations)
            {
                if (sim.Top is IStateful<ITimeSimulationState> t)
                    sim.States.Add(t.State);
            }
        }
    }
}
