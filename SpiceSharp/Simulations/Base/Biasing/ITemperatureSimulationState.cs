namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="ISimulationState"/> for tracking temperature.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface ITemperatureSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets the current temperature in Kelvin for this circuit in Kelvin.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        double Temperature { get; set; }

        /// <summary>
        /// Gets the nominal temperature in Kelvin for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        double NominalTemperature { get; set; }
    }
}
