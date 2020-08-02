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
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

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
                var exportargs = new ExportDataEventArgs(this);
                if (FrequencyParameters.KeepOpInfo)
                    OnExport(exportargs);

                // Sweep the frequency
                foreach (var freq in FrequencyParameters.Frequencies)
                {
                    // Calculate the current frequency
                    cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                    // Solve
                    AcIterate();

                    // Export the timepoint
                    OnExport(exportargs);
                }
            }
            finally
            {
                Statistics.ComplexTime.Stop();
            }
        }
    }
}
