namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a noise source.
    /// </summary>
    public interface INoiseSource
    {
        /// <summary>
        /// Gets the name of the noise source.
        /// </summary>
        /// <value>
        /// The name of the noise source.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the last calculated noise density.
        /// </summary>
        /// <value>
        /// The last calculated noise density.
        /// </value>
        double OutputNoiseDensity { get; }

        /// <summary>
        /// Gets the total integrated output noise.
        /// </summary>
        /// <value>
        /// The total integrated output noise.
        /// </value>
        double TotalOutputNoise { get; }

        /// <summary>
        /// Gets the total integrated input noise.
        /// </summary>
        /// <value>
        /// The total integrated input noise.
        /// </value>
        double TotalInputNoise { get; }

        /// <summary>
        /// Initializes the noise source. Resets all noise contributions to 0.
        /// </summary>
        void Initialize();
    }
}
