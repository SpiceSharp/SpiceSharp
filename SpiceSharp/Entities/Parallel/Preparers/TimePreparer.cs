using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A time preparer for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class TimePreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(IParallelSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<ITimeSimulationState>();
            taskSimulation.States.Add(state);
        }
    }
}
