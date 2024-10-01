using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements a frequency-domain analysis (AC analysis).
    /// </summary>
    /// <seealso cref="FrequencySimulation" />
    public class AC : FrequencySimulation
    {
        /// <summary>
        /// The constant returned when exporting the operating point.
        /// </summary>
        public const int ExportOperatingPoint = 0x01;

        /// <summary>
        /// The constant returned when exporting the small signal point.
        /// </summary>
        public const int ExportSmallSignal = 0x02;

        /// <summary>
        /// Gets the current frequency point.
        /// </summary>
        public double Frequency => GetState<IComplexSimulationState>().Laplace.Imaginary / (2 * Math.PI);

        /// <summary>
        /// Initializes a new instance of the <see cref="AC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public AC(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="frequencySweep">The frequency points.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public AC(string name, IEnumerable<double> frequencySweep)
            : base(name, frequencySweep)
        {
        }

        /// <inheritdoc/>
        protected override IEnumerable<int> Execute(int mask)
        {
            // Execute base behavior
            foreach (int exportType in base.Execute(mask))
                yield return exportType;

            var cstate = (ComplexSimulationState)GetState<IComplexSimulationState>();

            // Calculate the operating point
            cstate.Laplace = 0.0;
            Op(BiasingParameters.DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            Statistics.ComplexTime.Start();
            try
            {
                InitializeAcParameters();

                // Export operating point if requested
                if ((mask & ExportOperatingPoint) != 0)
                    yield return ExportOperatingPoint;

                // Sweep the frequency
                foreach (double freq in FrequencyParameters.Frequencies)
                {
                    // Calculate the current frequency
                    cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                    // Solve
                    AcIterate();

                    // Export the timepoint
                    if ((mask & ExportSmallSignal) != 0)
                        yield return ExportSmallSignal;
                }
            }
            finally
            {
                Statistics.ComplexTime.Stop();
            }
        }
    }
}
