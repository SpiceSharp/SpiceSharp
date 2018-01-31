using System;
using SpiceSharp.Diagnostics;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Frequency-domain analysis (AC analysis)
    /// </summary>
    public class AC : FrequencySimulation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        public AC(string name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="frequencySweep">Frequency sweep</param>
        public AC(Identifier name, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var circuit = Circuit;

            var state = State;
            var baseconfig = BaseConfiguration;
            var freqconfig = FrequencyConfiguration;
            
            // Calculate the operating point
            state.Initialize(circuit);
            state.Laplace = 0.0;
            state.Domain = State.DomainType.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Gmin = baseconfig.Gmin;
            Op(baseconfig.DCMaxIterations);

            // Load all in order to calculate the AC info for all devices
            state.UseDC = false;
            state.UseSmallSignal = true;
            foreach (var behavior in LoadBehaviors)
                behavior.Load(this);
            foreach (var behavior in FrequencyBehaviors)
                behavior.InitializeParameters(this);

            // Export operating point if requested
            var exportargs = new ExportDataEventArgs(State);
            if (freqconfig.KeepOpInfo)
                Export(exportargs);

            // Calculate the AC solution
            state.UseDC = false;
            state.Matrix.Complex = true;

            // Sweep the frequency
            foreach (double freq in FrequencySweep.Points)
            {
                // Calculate the current frequency
                state.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                // Solve
                ACIterate(circuit);

                // Export the timepoint
                Export(exportargs);
            }
        }
    }
}
