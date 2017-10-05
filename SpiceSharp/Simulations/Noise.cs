using System;
using System.Numerics;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using static SpiceSharp.Simulations.SimulationIterate;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Noise analysis
    /// </summary>
    public class Noise : Simulation<Noise>
    {
        /// <summary>
        /// Enumerations
        /// </summary>
        public enum StepTypes { Decade, Octave, Linear };

        /// <summary>
        /// Gets or sets the noise output node
        /// </summary>
        [SpiceName("output"), SpiceInfo("Noise output summation node")]
        public string Output { get; set; } = null;

        /// <summary>
        /// Gets or sets the noise output reference node
        /// </summary>
        [SpiceName("outputref"), SpiceInfo("Noise output reference node")]
        public string OutputRef { get; set; } = null;

        /// <summary>
        /// Gets or sets the name of the AC source used as input reference
        /// </summary>
        [SpiceName("input"), SpiceInfo("Name of the AC source used as input reference")]
        public ICircuitObject Input { get; set; } = null;

        /// <summary>
        /// Gets or sets the starting frequency
        /// </summary>
        [SpiceName("start"), SpiceInfo("Starting frequency")]
        public double StartFreq { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the stopping frequency
        /// </summary>
        [SpiceName("stop"), SpiceInfo("Stopping frequency")]
        public double StopFreq { get; set; } = 1.0e3;

        /// <summary>
        /// Gets or sets the number of steps
        /// </summary>
        [SpiceName("steps"), SpiceName("n"), SpiceInfo("The number of steps")]
        public double Steps
        {
            get => NumberSteps;
            set => NumberSteps = (int)Math.Round(value + 0.1);
        }
        public int NumberSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the step type (string version)
        /// </summary>
        [SpiceName("type"), SpiceInfo("The step type")]
        public string _StepType
        {
            get
            {
                switch (StepType)
                {
                    case StepTypes.Linear: return "lin";
                    case StepTypes.Octave: return "oct";
                    case StepTypes.Decade: return "dec";
                }
                return null;
            }
            set
            {
                switch (value.ToLower())
                {
                    case "linear":
                    case "lin": StepType = StepTypes.Linear; break;
                    case "octave":
                    case "oct": StepType = StepTypes.Octave; break;
                    case "decade":
                    case "dec": StepType = StepTypes.Decade; break;
                    default:
                        throw new CircuitException($"Invalid step type {value}");
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of step used
        /// </summary>
        public StepTypes StepType { get; set; } = StepTypes.Decade;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public Noise(string name) : base(name)
        {
        }

        /// <summary>
        /// Execute the noise analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Execute(Circuit ckt)
        {
            var state = ckt.State;
            var config = CurrentConfig;

            // Find the output nodes
            int posOutNode = !string.IsNullOrWhiteSpace(Output) ? ckt.Nodes[Output].Index : 0;
            int negOutNode = !string.IsNullOrWhiteSpace(OutputRef) ? ckt.Nodes[OutputRef].Index : 0;

            // See if the source specified is AC
            if (Input is Voltagesource)
            {
                if (!(Input as Voltagesource).VSRCacMag.Given)
                    throw new CircuitException($"{Name}: Noise input source {Input.Name} has no AC input");
            }
            else if (Input is Currentsource)
            {
                if (!(Input as Currentsource).ISRCacMag.Given)
                    throw new CircuitException($"{Name}: Noise input source {Input.Name} has no AC input");
            }
            else
                throw new CircuitException($"{Name}: Invalid input noise source");

            double freqdelta = 0.0;
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

            // Initialize
            state.Initialize(ckt);
            ckt.State.Noise.Initialize(StartFreq);
            state.Complex.Laplace = 0;
            state.Domain = CircuitState.DomainTypes.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Gmin = config.Gmin;
            Initialize(ckt);
            Op(config, ckt, config.DcMaxIterations);

            var data = ckt.State.Noise;
            var cstate = ckt.State.Complex;

            // Loop through noise figures
            for (int i = 0; i < n; i++)
            {
                cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * data.Freq);
                AcIterate(config, ckt);

                Complex val = cstate.Solution[posOutNode] - cstate.Solution[negOutNode];
                data.GainSqInv = 1.0 / Math.Max(val.Real * val.Real + val.Imaginary * val.Imaginary, 1e-20);

                // Solve the adjoint system
                NzIterate(ckt, posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                data.outNdens = 0.0;
                foreach (var o in ckt.Objects)
                    o.Noise(ckt);

                // Export the data
                Export(ckt);

                // Increment the frequency
                switch (StepType)
                {
                    case StepTypes.Decade:
                    case StepTypes.Octave:
                        data.Freq = data.Freq * freqdelta;
                        break;

                    case StepTypes.Linear:
                        data.Freq = StartFreq + i * freqdelta;
                        break;
                }
            }

            Finalize(ckt);
        }
    }
}
