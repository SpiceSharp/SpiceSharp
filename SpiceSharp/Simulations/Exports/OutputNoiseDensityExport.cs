namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Export for the output noise density
    /// </summary>
    public class OutputNoiseDensityExport : Export<double>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noise">Noise analysis</param>
        public OutputNoiseDensityExport(Noise noise)
            : base(noise)
        {
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected override void Initialize(object sender, InitializeSimulationEventArgs e)
        {
            var noise = (Noise)Simulation;
            Extractor = () => noise.NoiseState.OutputNoiseDensity;
        }
    }
}
