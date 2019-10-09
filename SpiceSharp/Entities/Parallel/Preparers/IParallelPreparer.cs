using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An interface that can prepare simulations for a <see cref="ParallelEntity"/>.
    /// </summary>
    public interface IParallelPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        void Prepare(IParallelSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters);
    }
}
