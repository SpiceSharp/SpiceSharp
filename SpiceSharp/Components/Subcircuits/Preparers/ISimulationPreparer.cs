using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An interface that can prepare a <see cref="SubcircuitSimulation"/> for a specified behavior type.
    /// </summary>
    public interface ISimulationPreparer
    {
        /// <summary>
        /// Prepares the task's simulation for the behavior.
        /// </summary>
        /// <param name="taskSimulation">The task simulation to be prepared.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters.</param>
        void Prepare(SubcircuitSimulation taskSimulation, ISimulation parent, ParameterSetDictionary parameters);
    }
}
