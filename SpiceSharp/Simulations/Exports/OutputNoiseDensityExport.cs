using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export the output noise density.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class OutputNoiseDensityExport : Export<double>
    {
        /// <summary>
        /// Check if the simulation is a <see cref="Noise"/> simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        protected override bool IsValidSimulation(Simulation simulation) => simulation is Noise;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputNoiseDensityExport"/> class.
        /// </summary>
        /// <param name="noise">The noise analysis.</param>
        public OutputNoiseDensityExport(Noise noise)
            : base(noise)
        {
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            var noise = (Noise)Simulation;
            Extractor = () => noise.NoiseState.OutputNoiseDensity;
        }
    }
}
