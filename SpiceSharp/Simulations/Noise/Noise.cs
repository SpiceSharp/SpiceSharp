using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Behaviors;

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
        public CircuitIdentifier Output { get; set; } = null;

        /// <summary>
        /// Gets or sets the noise output reference node
        /// </summary>
        [SpiceName("outputref"), SpiceInfo("Noise output reference node")]
        public CircuitIdentifier OutputRef { get; set; } = null;

        /// <summary>
        /// Gets or sets the name of the AC source used as input reference
        /// </summary>
        [SpiceName("input"), SpiceInfo("Name of the AC source used as input reference")]
        public CircuitIdentifier Input { get; set; } = null;

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
        /// Private variables
        /// </summary>
        private List<CircuitObjectBehaviorLoad> loadbehaviours;
        private List<CircuitObjectBehaviorAcLoad> acbehaviours;
        private List<CircuitObjectBehaviorNoise> noisebehaviours;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public Noise(string name) : base(name)
        {
        }

        /// <summary>
        /// Initialize the noise simulation
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Initialize(Circuit ckt)
        {
            base.Initialize(ckt);

            // Get all behaviours necessary for noise analysis
            loadbehaviours = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorLoad>(ckt);
            acbehaviours = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorAcLoad>(ckt);
            noisebehaviours = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorNoise>(ckt);
        }


        /// <summary>
        /// Execute the noise analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        protected override void Execute()
        {
            var ckt = Circuit;
            var state = ckt.State;
            var config = CurrentConfig;

            // Find the output nodes
            int posOutNode = Output != null ? ckt.Nodes[Output].Index : 0;
            int negOutNode = OutputRef != null ? ckt.Nodes[OutputRef].Index : 0;

            // Check the voltage or current source
            if (Input == null)
                throw new CircuitException($"{Name}: No input source specified");
            ICircuitObject source = ckt.Objects[Input];
            if (source is Voltagesource vsource)
            {
                if (!vsource.VSRCacMag.Given || vsource.VSRCacMag == 0.0)
                    throw new CircuitException($"{Name}: Noise input source {vsource.Name} has no AC input");
            }
            else if (source is Currentsource isource)
            {
                if (!isource.ISRCacMag.Given || isource.ISRCacMag == 0.0)
                    throw new CircuitException($"{Name}: Noise input source {isource.Name} has not AC input");
            }
            else
                throw new CircuitException($"{Name}: No input source");

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
            state.Laplace = 0;
            state.Domain = CircuitState.DomainTypes.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Gmin = config.Gmin;
            Initialize(ckt);
            ckt.Op(loadbehaviours, config, config.DcMaxIterations);

            var data = ckt.State.Noise;
            state.Sparse |= CircuitState.SparseFlags.NIACSHOULDREORDER;

            // Loop through noise figures
            for (int i = 0; i < n; i++)
            {
                state.Laplace = new Complex(0.0, 2.0 * Math.PI * data.Freq);
                ckt.AcIterate(acbehaviours, config);

                double rval = state.Solution[posOutNode] - state.Solution[negOutNode];
                double ival = state.iSolution[posOutNode] - state.iSolution[negOutNode];
                data.GainSqInv = 1.0 / Math.Max(rval * rval + ival * ival, 1e-20);

                // Solve the adjoint system
                ckt.NzIterate(posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                data.outNdens = 0.0;
                foreach (var behaviour in noisebehaviours)
                    behaviour.Execute(ckt);

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
