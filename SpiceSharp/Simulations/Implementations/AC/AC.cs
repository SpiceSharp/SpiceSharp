using SpiceSharp.Entities;
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
        private bool _keepOpInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        public AC(string name) : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public AC(string name, IEnumerable<double> frequencySweep) : base(name, frequencySweep)
        {
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(IEntityCollection entities)
        {
            var config = Configurations.GetValue<FrequencyConfiguration>();
            _keepOpInfo = config.KeepOpInfo;

            base.Setup(entities);
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var cstate = ComplexState;
            
            // Calculate the operating point
            cstate.Laplace = 0.0;
            BiasingState.UseIc = false;
            BiasingState.UseDc = true;
            Op(DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            Statistics.ComplexTime.Start();
            try
            {
                InitializeAcParameters();

                // Export operating point if requested
                var exportargs = new ExportDataEventArgs(this);
                if (_keepOpInfo)
                    OnExport(exportargs);

                // Sweep the frequency
                foreach (var freq in Frequencies)
                {
                    // Calculate the current frequency
                    cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                    // Solve
                    AcIterate();

                    // Export the timepoint
                    OnExport(exportargs);
                }

                Statistics.ComplexTime.Stop();
            }
            catch (Exception)
            {
                Statistics.ComplexTime.Stop();
                throw;
            }
        }
    }
}
