namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state for a <see cref="Noise"/> analysis.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface INoiseSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets the total output noise density of all noise sources in the circuit.
        /// </summary>
        /// <value>
        /// The total output noise density.
        /// </value>
        double OutputNoiseDensity { get; }

        /// <summary>
        /// Gets the total integrated output noise of all noise sources in the circuit.
        /// </summary>
        /// <value>
        /// The total integrated output noise.
        /// </value>
        double TotalOutputNoise { get; }

        /// <summary>
        /// Gets the total integrated input noise of all noise sources in the circuit.
        /// </summary>
        /// <value>
        /// The total integrated input noise.
        /// </value>
        double TotalInputNoise { get; }

        /// <summary>
        /// Gets the history of input data points. The index 0 contains the current point.
        /// </summary>
        /// <value>
        /// The history of data points.
        /// </value>
        IHistory<NoisePoint> Point { get; }
    }
}
