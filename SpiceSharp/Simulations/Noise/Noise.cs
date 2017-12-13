using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.ComponentBehaviors;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Noise analysis
    /// </summary>
    public class Noise : FrequencySimulation
    {
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
        /// Private variables
        /// </summary>
        private List<CircuitObjectBehaviorNoise> noisebehaviors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public Noise(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors
            noisebehaviors = SetupBehaviors<CircuitObjectBehaviorNoise>();
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            foreach (var behavior in noisebehaviors)
                behavior.Unsetup();
            noisebehaviors.Clear();
            noisebehaviors = null;

            base.Unsetup();
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
            CircuitObject source = ckt.Objects[Input];
            if (source is Voltagesource vsource)
            {
                var ac = vsource.GetBehavior(typeof(CircuitObjectBehaviorAcLoad)) as VoltageSourceLoadAcBehavior;
                if (!ac.VSRCacMag.Given || ac.VSRCacMag == 0.0)
                    throw new CircuitException($"{Name}: Noise input source {vsource.Name} has no AC input");
            }
            else if (source is Currentsource isource)
            {
                var ac = isource.GetBehavior(typeof(CircuitObjectBehaviorAcLoad)) as CurrentsourceAcBehavior;
                if (!ac.ISRCacMag.Given || ac.ISRCacMag == 0.0)
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
            Op(ckt, config.DcMaxIterations);

            var data = ckt.State.Noise;
            state.Sparse |= CircuitState.SparseFlags.NIACSHOULDREORDER;

            // Loop through noise figures
            for (int i = 0; i < n; i++)
            {
                state.Laplace = new Complex(0.0, 2.0 * Math.PI * data.Freq);
                AcIterate(ckt);

                double rval = state.Solution[posOutNode] - state.Solution[negOutNode];
                double ival = state.iSolution[posOutNode] - state.iSolution[negOutNode];
                data.GainSqInv = 1.0 / Math.Max(rval * rval + ival * ival, 1e-20);

                // Solve the adjoint system
                NzIterate(ckt, posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                data.outNdens = 0.0;
                foreach (var behaviour in noisebehaviors)
                    behaviour.Noise(ckt);

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
        }

        /// <summary>
        /// Calculate the solution for <see cref="Noise"/> analysis
        /// This routine solves the adjoint system. It assumes that the matrix has
        /// already been loaded by a call to AcIterate, so it only alters the right
        /// hand side vector. The unit-valued current excitation is applied between
        /// nodes posDrive and negDrive.
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="posDrive">The positive driving node</param>
        /// <param name="negDrive">The negative driving node</param>
        private void NzIterate(Circuit ckt, int posDrive, int negDrive)
        {
            var state = ckt.State;

            // Clear out the right hand side vector
            for (int i = 0; i < state.Rhs.Length; i++)
            {
                state.Rhs[i] = 0.0;
                state.iRhs[i] = 0.0;
            }

            // Apply unit current excitation
            state.Rhs[posDrive] = 1.0;
            state.Rhs[negDrive] = -1.0;

            state.Matrix.SolveTransposed(state.Rhs, state.iRhs);

            state.StoreSolution(true);

            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;
        }
    }
}
