namespace SpiceSharp.Simulations.Biasing
{
    /// <summary>
    /// An <see cref="ITemperatureSimulationState"/> for an <see cref="ITemperatureSimulation"/>.
    /// </summary>
    /// <seealso cref="ITemperatureSimulationState" />
    public class TemperatureSimulationState : ITemperatureSimulationState
    {
        /// <inheritdoc/>
        public double Temperature { get; set; }

        /// <inheritdoc/>
        public double NominalTemperature { get; set; }

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
