namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state for a <see cref="Noise"/> analysis.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface INoiseSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets or sets the current frequency.
        /// </summary>
        double Frequency { get; }

        /// <summary>
        /// Gets or sets the frequency step.
        /// </summary>
        double DeltaFrequency { get; }

        /// <summary>
        /// Output referred noise
        /// </summary>
        double OutputNoise { get; set; }

        /// <summary>
        /// Gets or sets the total input-referred noise.
        /// </summary>
        double InputNoise { get; set; }

        /// <summary>
        /// Gets or sets the total output noise density.
        /// </summary>
        double OutputNoiseDensity { get; set; }

        /// <summary>
        /// Gets or sets the inverse squared gain.
        /// </summary>
        /// <remarks>
        /// This value is used to compute the input noise density from the output noise density.
        /// </remarks>
        double GainInverseSquared { get; }

        /// <summary>
        /// Gets the logarithm of the gain squared.
        /// </summary>
        double LogInverseGain { get; }

        /// <summary>
        /// This subroutine evaluate the integration of the function
        /// NOISE = a * (FREQUENCY) ^ (EXPONENT)
        /// given two points from the curve. If EXPONENT is relatively close to 0, the noise is simply multiplied
        /// by the change in frequency.
        /// If it isn't, a more complicated expression must be used.
        /// Note that EXPONENT = -1 gives a different equation than EXPONENT != -1.
        /// </summary>
        /// <param name="noiseDensity">The noise density.</param>
        /// <param name="logNoiseDensity">The previous noise density</param>
        /// <param name="lastLogNoiseDensity">The previous log noise density</param>
        /// <returns>
        /// The integrated noise.
        /// </returns>
        double Integrate(double noiseDensity, double logNoiseDensity, double lastLogNoiseDensity);
    }
}
