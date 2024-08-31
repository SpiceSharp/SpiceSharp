using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export the output-referred noise density.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class OutputNoiseDensityExport : Export<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputNoiseDensityExport"/> class.
        /// </summary>
        /// <param name="noise">The noise analysis.</param>
        public OutputNoiseDensityExport(INoiseSimulation noise)
            : base(noise)
        {
        }

        /// <inheritdoc />
        protected override Func<double> BuildExtractor(ISimulation simulation)
        {
            if (simulation is not null &&
                simulation.TryGetState<INoiseSimulationState>(out var state))
                return () => state.OutputNoiseDensity;
            return null;
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => "onoise()";
    }
}
