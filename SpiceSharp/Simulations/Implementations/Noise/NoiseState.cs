using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that represents the state of a <see cref="Noise" /> analysis.
    /// </summary>
    /// <seealso cref="SimulationState" />
    public class NoiseState : SimulationState
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private double _gainSquareInverted, _currentFrequency, _lastFrequency, _logLastFrequency, _deltaFrequency, _deltaLogFrequency, _logFrequency;

        /// <summary>
        /// Gets or sets the current frequency.
        /// </summary>
        public double Frequency
        {
            get => _currentFrequency;
            set
            {
                // Shift current frequency to last frequency
                _lastFrequency = _currentFrequency;
                _logLastFrequency = _logFrequency;

                // Update new values
                _currentFrequency = value;
                _logFrequency = Math.Log(Math.Max(_currentFrequency, 1e-38));

                // Delta
                _deltaFrequency = _currentFrequency - _lastFrequency;
                _deltaLogFrequency = _logFrequency - _logLastFrequency;
            }
        }

        /// <summary>
        /// Gets or sets the frequency step.
        /// </summary>
        public double DeltaFrequency => _deltaFrequency;

        /// <summary>
        /// Output referred noise
        /// </summary>
        public double OutputNoise { get; set; }

        /// <summary>
        /// Gets or sets the total input-referred noise.
        /// </summary>
        public double InputNoise { get; set; }

        /// <summary>
        /// Gets or sets the total output noise density.
        /// </summary>
        public double OutputNoiseDensity { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the inverse squared gain.
        /// </summary>
        /// <remarks>
        /// This value is used to compute the input noise density from the output noise density.
        /// </remarks>
        public double GainInverseSquared
        {
            get => _gainSquareInverted;
            set
            {
                _gainSquareInverted = value;
                LogInverseGain = Math.Log(value);
            }
        }

        /// <summary>
        /// Gets the logarithm of the gain squared.
        /// </summary>
        public double LogInverseGain { get; private set; }

        /// <summary>
        /// Reset the frequency.
        /// </summary>
        /// <param name="frequency">The new frequency point.</param>
        public void Reset(double frequency)
        {
            // Set the current and last frequency
            _currentFrequency = frequency;
            _lastFrequency = frequency;

            // Reset integrated noise
            OutputNoise = 0;
            InputNoise = 0;
        }
        
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
        public double Integrate(double noiseDensity, double logNoiseDensity, double lastLogNoiseDensity)
        {
            var exponent = (logNoiseDensity - lastLogNoiseDensity) / _deltaLogFrequency;
            if (Math.Abs(exponent) < 1e-10)
                return noiseDensity * _deltaFrequency;
            var a = Math.Exp(logNoiseDensity - exponent * _logFrequency);
            exponent += 1.0;
            if (Math.Abs(exponent) < 1e-10)
                return a * (_logFrequency - _logLastFrequency);
            return a * (Math.Exp(exponent * _logFrequency) - Math.Exp(exponent * _logLastFrequency)) / exponent;
        }
    }
}
