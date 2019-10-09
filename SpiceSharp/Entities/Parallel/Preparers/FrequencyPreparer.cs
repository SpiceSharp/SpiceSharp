using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An <see cref="IParallelPreparer"/> for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class FrequencyPreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(IParallelSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<IComplexSimulationState>();
            taskSimulation.States.Add<IComplexSimulationState>(new ComplexSimulationState(state, taskSimulation.Task));
        }
    }
}
