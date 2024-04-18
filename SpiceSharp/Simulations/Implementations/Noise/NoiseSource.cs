using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A basic implementation of a <see cref="INoiseSource"/>.
    /// </summary>
    /// <seealso cref="INoiseSource" />
    public abstract class NoiseSource : INoiseSource
    {
        private double _lnLastOutputNoiseDensity;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public double OutputNoiseDensity { get; protected set; }

        /// <inheritdoc/>
        public double TotalOutputNoise { get; private set; }

        /// <inheritdoc/>
        public double TotalInputNoise { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseSource"/> class.
        /// </summary>
        /// <param name="name">The name of the noise source.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected NoiseSource(string name)
        {
            Name = name.ThrowIfNull(nameof(name));
        }

        /// <summary>
        /// Resets all the integrated noise, and uses the 
        /// output noise density as the initial point.
        /// </summary>
        public virtual void Initialize()
        {
            TotalOutputNoise = 0;
            TotalInputNoise = 0;
            _lnLastOutputNoiseDensity = Math.Log(Math.Max(OutputNoiseDensity, 1e-38));
        }

        /// <summary>
        /// Integrates the noise density into the total integrated noise figures.
        /// It computes the integration assuming that noise = a * frequency^exponent. It
        /// automatically tracks the noise density from one point to the next.
        /// </summary>
        /// <param name="state">The noise simulation state.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the simulation state does not store enough points in history.</exception>
        public virtual void Integrate(INoiseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));

            double lnOutputNoiseDensity = Math.Log(Math.Max(OutputNoiseDensity, 1e-38));
            double lnFrequency = state.Point.Value.LogFrequency;
            double lnLastFrequency = state.Point.GetPreviousValue(1).LogFrequency;
            double exponent = (lnOutputNoiseDensity - _lnLastOutputNoiseDensity) / (lnFrequency - lnLastFrequency);
            double delta;

            // Use simple box integration if the noise contribution doesn't change significantly over the frequency
            if (Math.Abs(exponent) < 1e-10)
            {
                delta = state.Point.Value.Frequency - state.Point.GetPreviousValue(1).Frequency;
                TotalOutputNoise += OutputNoiseDensity * delta;
                TotalInputNoise += OutputNoiseDensity * state.Point.Value.InverseGainSquared * delta;
                return;
            }

            exponent += 1.0;

            // Is the noise ~ 1/f?
            if (Math.Abs(exponent) < 1e-10)
                delta = lnFrequency - lnLastFrequency;
            else
                delta = (Math.Exp(exponent * lnFrequency) - Math.Exp(exponent * lnLastFrequency)) / exponent;

            // Compute total output noise
            double a = Math.Exp(lnOutputNoiseDensity - exponent * lnFrequency);
            TotalOutputNoise += a * delta;

            // Compute total input noise
            a = Math.Exp(lnOutputNoiseDensity + state.Point.Value.LogInverseGainSquared - exponent * lnFrequency);
            TotalInputNoise += a * (lnFrequency - lnLastFrequency);

            _lnLastOutputNoiseDensity = lnOutputNoiseDensity;
        }
    }
}
