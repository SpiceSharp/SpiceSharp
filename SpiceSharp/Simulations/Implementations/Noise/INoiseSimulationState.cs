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
        /// <value>
        /// The frequency.
        /// </value>
        double Frequency { get; }

        /// <summary>
        /// Gets or sets the frequency step.
        /// </summary>
        /// <value>
        /// The frequency step.
        /// </value>
        double DeltaFrequency { get; }

        /// <summary>
        /// Gets or sets the total output referred noise
        /// </summary>
        /// <value>
        /// The total output referred noise.
        /// </value>
        double OutputNoise { get; set; }

        /// <summary>
        /// Gets or sets the total input-referred noise.
        /// </summary>
        /// <value>
        /// The total input referred noise.
        /// </value>
        double InputNoise { get; set; }

        /// <summary>
        /// Gets or sets the total output noise density.
        /// </summary>
        /// <value>
        /// The total output noise density.
        /// </value>
        double OutputNoiseDensity { get; set; }

        /// <summary>
        /// Gets or sets the inverse squared gain.
        /// </summary>
        /// <remarks>
        /// This value is used to compute the input noise density from the output noise density.
        /// </remarks>
        /// <value>
        /// The inverse gain squared.
        /// </value>
        double GainInverseSquared { get; }

        /// <summary>
        /// Gets the logarithm of the gain squared.
        /// </summary>
        /// <value>
        /// The natural logarithm of the inverse gain.
        /// </value>
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
