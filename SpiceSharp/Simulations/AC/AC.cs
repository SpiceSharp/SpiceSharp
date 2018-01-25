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
        /// <param name="name">The name of the simulation</param>
        /// <param name="type">The simulation type: lin, oct or dec</param>
        /// <param name="n">The number of steps</param>
        /// <param name="start">The starting frequency</param>
        /// <param name="stop">The stopping frequency</param>
        public AC(Identifier name, string type, int n, double start, double stop) : base(name, type, n, start, stop)
        {
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var ckt = Circuit;

            var state = State;
            var cstate = state;
            var baseconfig = BaseConfiguration;
            var freqconfig = FrequencyConfiguration;

            double freq = 0.0, freqdelta = 0.0;
            int n = 0;

            // Calculate the step
            switch (freqconfig.StepType)
            {
                case StepTypes.Decade:
                    freqdelta = Math.Exp(Math.Log(10.0) / freqconfig.NumberSteps);
                    n = (int)Math.Floor(Math.Log(freqconfig.StopFreq / freqconfig.StartFreq) / Math.Log(freqdelta) + 0.25) + 1;
                    break;

                case StepTypes.Octave:
                    freqdelta = Math.Exp(Math.Log(2.0) / freqconfig.NumberSteps);
                    n = (int)Math.Floor(Math.Log(freqconfig.StopFreq / freqconfig.StartFreq) / Math.Log(freqdelta) + 0.25) + 1;
                    break;

                case StepTypes.Linear:
                    if (freqconfig.NumberSteps > 1)
                    {
                        freqdelta = (freqconfig.StopFreq - freqconfig.StartFreq) / (freqconfig.NumberSteps - 1);
                        n = freqconfig.NumberSteps;
                    }
                    else
                    {
                        freqdelta = double.PositiveInfinity;
                        n = 1;
                    }
                    break;

                default:
                    throw new CircuitException("Invalid step type");
            }

            // Calculate the operating point
            state.Initialize(ckt);
            state.Laplace = 0.0;
            state.Domain = State.DomainTypes.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Gmin = baseconfig.Gmin;
            Op(baseconfig.DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            state.UseDC = false;
            state.UseSmallSignal = true;
            foreach (var behavior in loadbehaviors)
                behavior.Load(this);
            foreach (var behavior in acbehaviors)
                behavior.InitializeParameters(this);

            // Export operating point if requested
            var exportargs = new ExportDataEventArgs(State);
            if (freqconfig.KeepOpInfo)
                Export(exportargs);

            // Calculate the AC solution
            state.UseDC = false;
            freq = freqconfig.StartFreq;
            state.Matrix.Complex = true;

            // Sweep the frequency
            for (int i = 0; i < n; i++)
            {
                // Calculate the current frequency
                state.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                // Solve
                AcIterate(ckt);

                // Export the timepoint
                Export(exportargs);

                // Increment the frequency
                switch (freqconfig.StepType)
                {
                    case StepTypes.Decade:
                    case StepTypes.Octave:
                        freq = freq * freqdelta;
                        break;

                    case StepTypes.Linear:
                        freq = freqconfig.StartFreq + i * freqdelta;
                        break;
                }
            }
        }
    }
}
