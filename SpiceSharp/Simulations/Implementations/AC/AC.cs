using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements a frequency-domain analysis (AC analysis).
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.FrequencySimulation" />
    public class AC : FrequencySimulation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        public AC(Identifier name) : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public AC(Identifier name, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var state = RealState;
            var cstate = ComplexState;
            var baseconfig = BaseConfiguration;
            var freqconfig = FrequencyConfiguration;
            
            // Calculate the operating point
            cstate.Laplace = 0.0;
            state.UseIc = false;
            state.UseDc = true;
            Op(baseconfig.DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            InitializeAcParameters();

            // Export operating point if requested
            var exportargs = new ExportDataEventArgs(this);
            if (freqconfig.KeepOpInfo)
                OnExport(exportargs);

            // Sweep the frequency
            foreach (var freq in FrequencySweep.Points)
            {
                // Calculate the current frequency
                cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                // Solve
                AcIterate();

                // Export the timepoint
                OnExport(exportargs);
            }
        }
    }
}
