using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export the total input-referred noise density.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class InputNoiseDensityExport : Export<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputNoiseDensityExport"/> class.
        /// </summary>
        /// <param name="noise">The noise analysis.</param>
        public InputNoiseDensityExport(INoiseSimulation noise)
            : base(noise)
        {
        }

        /// <inheritdoc />
        protected override Func<double> BuildExtractor(ISimulation simulation)
        {
            if (simulation is not null &&
                simulation.TryGetState<INoiseSimulationState>(out var state))
                return () => state.OutputNoiseDensity * state.Point.Value.InverseGainSquared;
            return null;
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "inoise()";
    }
}
