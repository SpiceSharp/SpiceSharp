using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Noise preparer for a <see cref="ParallelEntity"/>
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class NoisePreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        public void Prepare(IParallelSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters)
        {
            var state = parent.States.GetValue<INoiseSimulationState>();
            taskSimulation.States.Add(state);
        }
    }
}
