namespace SpiceSharp.Simulations.Biasing
{
    /// <summary>
    /// An <see cref="ITemperatureSimulationState"/> for an <see cref="ITemperatureSimulation"/>.
    /// </summary>
    /// <seealso cref="ITemperatureSimulationState" />
    public class TemperatureSimulationState : ITemperatureSimulationState
    {
        /// <summary>
        /// Gets or ets the current temperature in Kelvin for this circuit in Kelvin.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public double Temperature { get; set; }

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// It can be used by models as the default temperature where the parameters were measured.
        /// </summary>
        public double NominalTemperature { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureSimulationState"/> class.
        /// </summary>
        /// <param name="temperature">The temperature.</param>
        /// <param name="nominalTemperature">The nominal temperature.</param>
        public TemperatureSimulationState(double temperature, double nominalTemperature)
        {
            Temperature = temperature;
            NominalTemperature = nominalTemperature;
        }
    }
}
