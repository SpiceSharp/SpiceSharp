using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export the input-referred noise density.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class InputNoiseDensityExport : Export<double>
    {
        /// <summary>
        /// Check if the simulation is a <see cref="Noise"/> simulation.
        /// </summary>
        /// <param name="simulation"></param>
        /// <returns></returns>
        protected override bool IsValidSimulation(ISimulation simulation) => simulation is Noise;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputNoiseDensityExport"/> class.
        /// </summary>
        /// <param name="noise">The noise analysis.</param>
        public InputNoiseDensityExport(Noise noise)
            : base(noise)
        {
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            var simulation = (Simulation)sender;
            var state = simulation.States.GetValue<INoiseSimulationState>();
            Extractor = () => state.OutputNoiseDensity * state.GainInverseSquared;
        }
    }
}
