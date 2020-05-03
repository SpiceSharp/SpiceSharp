using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export the input-referred noise density.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class InputNoiseDensityExport : Export<INoiseSimulation, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputNoiseDensityExport"/> class.
        /// </summary>
        /// <param name="noise">The noise analysis.</param>
        public InputNoiseDensityExport(INoiseSimulation noise)
            : base(noise)
        {
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            var state = Simulation.GetState<INoiseSimulationState>();
            Extractor = () => state.OutputNoiseDensity * state.Point.Value.InverseGainSquared;
        }
    }
}
