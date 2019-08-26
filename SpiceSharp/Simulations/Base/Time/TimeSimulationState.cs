using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes the state of a <see cref="TimeSimulation"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.SimulationState" />
    public class TimeSimulationState : SimulationState
    {
        /// <summary>
        /// Gets the integration method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public IntegrationMethod Method { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulationState"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        public TimeSimulationState(IntegrationMethod method)
        {
            Method = method.ThrowIfNull(nameof(method));
        }
    }
}
