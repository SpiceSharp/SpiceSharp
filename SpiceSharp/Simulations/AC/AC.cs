using System;
using SpiceSharp.Circuits;
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
        public AC(Identifier name, string type, int n, double start, double stop) : base(name)
        {
            switch (type.ToLower())
            {
                case "dec": StepType = StepTypes.Decade; break;
                case "oct": StepType = StepTypes.Octave; break;
                case "lin": StepType = StepTypes.Linear; break;
            }
            NumberSteps = n;
            StartFreq = start;
            StopFreq = stop;
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var ckt = Circuit;

            var state = ckt.State;
            var cstate = state;
            var config = CurrentConfig;

            double freq = 0.0, freqdelta = 0.0;
            int n = 0;

            // Calculate the step
            switch (StepType)
            {
                case StepTypes.Decade:
                    freqdelta = Math.Exp(Math.Log(10.0) / NumberSteps);
                    n = (int)Math.Floor(Math.Log(StopFreq / StartFreq) / Math.Log(freqdelta) + 0.25) + 1;
                    break;

                case StepTypes.Octave:
                    freqdelta = Math.Exp(Math.Log(2.0) / NumberSteps);
                    n = (int)Math.Floor(Math.Log(StopFreq / StartFreq) / Math.Log(freqdelta) + 0.25) + 1;
                    break;

                case StepTypes.Linear:
                    if (NumberSteps > 1)
                    {
                        freqdelta = (StopFreq - StartFreq) / (NumberSteps - 1);
                        n = NumberSteps;
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
            state.Gmin = config.Gmin;
            Op(ckt, config.DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            state.UseDC = false;
            state.UseSmallSignal = true;
            foreach (var behavior in loadbehaviors)
                behavior.Load(ckt);
            foreach (var behavior in acbehaviors)
                behavior.InitializeParameters(this);

            // Export operating point if requested
            if (config.KeepOpInfo)
                Export(ckt);

            // Calculate the AC solution
            state.UseDC = false;
            freq = StartFreq;
            ckt.State.Matrix.Complex = true;

            // Sweep the frequency
            for (int i = 0; i < n; i++)
            {
                // Calculate the current frequency
                state.Laplace = new Complex(0.0, 2.0 * Circuit.CONSTPI * freq);

                // Solve
                AcIterate(ckt);

                // Export the timepoint
                Export(ckt);

                // Increment the frequency
                switch (StepType)
                {
                    case StepTypes.Decade:
                    case StepTypes.Octave:
                        freq = freq * freqdelta;
                        break;

                    case StepTypes.Linear:
                        freq = StartFreq + i * freqdelta;
                        break;
                }
            }
        }
    }
}
