using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Simulation state for a <see cref="TimeSimulation"/>.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface ITimeSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets the integration method.
        /// </summary>
        /// <value>
        /// The integration method.
        /// </value>
        IntegrationMethod Method { get; }
    }
}
