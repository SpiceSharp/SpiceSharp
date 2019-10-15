using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;

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
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(SubcircuitSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<IComplexSimulationState>();
            if (parameters.TryGetValue<FrequencyParameters>(out var fp) &&
                fp.ParallelLoad && taskSimulation.Tasks > 1)
            {
                taskSimulation.States.Add<IComplexSimulationState>(
                    new ComplexSimulationState(
                        state,
                        new SolverElementProvider<Complex>(state.Solver)
                        )
                    );
            }
            else
                taskSimulation.States.Add(state);
        }
    }
}
