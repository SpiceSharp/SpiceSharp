using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Export for the input-referred noise density
    /// </summary>
    public class InputNoiseDensityExport : Export<double>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noise">Noise analysis</param>
        public InputNoiseDensityExport(Noise noise)
            : base(noise)
        {
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            Noise noise = (Noise)Simulation;
            Extractor = () => noise.NoiseState.OutputNoiseDensity * noise.NoiseState.GainInverseSquared;
        }
    }
}
