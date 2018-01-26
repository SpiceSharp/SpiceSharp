using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Behaviors;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Noise analysis
    /// </summary>
    public class Noise : FrequencySimulation
    {
        /// <summary>
        /// Gets the currently active noise configuration
        /// </summary>
        public NoiseConfiguration NoiseConfiguration { get; protected set; }

        /// <summary>
        /// Get the noise state
        /// </summary>
        public StateNoise NoiseState { get; } = new StateNoise();

        /// <summary>
        /// Private variables
        /// </summary>
        List<NoiseBehavior> noisebehaviors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public Noise(Identifier name) : base(name)
        {
            Parameters.Add(new NoiseConfiguration());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="input">Input</param>
        /// <param name="output">Output</param>
        /// <param name="type">Step type</param>
        /// <param name="n">Steps</param>
        /// <param name="start">Start</param>
        /// <param name="stop">Stop</param>
        public Noise(Identifier name, Identifier output, Identifier input, string type, int n, double start, double stop) : base(name, type, n, start, stop)
        {
            Parameters.Add(new NoiseConfiguration(output, null, input));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="input">Input</param>
        /// <param name="output">Output</param>
        /// <param name="reference">Output reference</param>
        /// <param name="type">Type</param>
        /// <param name="n">Steps</param>
        /// <param name="start">Start</param>
        /// <param name="stop">Stop</param>
        public Noise(Identifier name, Identifier output, Identifier reference, Identifier input, string type, int n, double start, double stop) : base(name, type, n, start, stop)
        {
            Parameters.Add(new NoiseConfiguration(output, reference, input));
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors and configurations
            NoiseConfiguration = Parameters.Get<NoiseConfiguration>();
            noisebehaviors = SetupBehaviors<NoiseBehavior>();
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
            NoiseConfiguration = null;

            base.Unsetup();
        }

        /// <summary>
        /// Execute the noise analysis
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            var circuit = Circuit;
            var state = State;
            var noiseconfig = NoiseConfiguration;
            var freqconfig = FrequencyConfiguration;
            var baseconfig = BaseConfiguration;
            var exportargs = new ExportDataEventArgs(State);

            // Find the output nodes
            int posOutNode = noiseconfig.Output != null ? circuit.Nodes[noiseconfig.Output].Index : 0;
            int negOutNode = noiseconfig.OutputRef != null ? circuit.Nodes[noiseconfig.OutputRef].Index : 0;

            // Check the voltage or current source
            if (noiseconfig.Input == null)
                throw new CircuitException($"{Name}: No input source specified");
            Entity source = circuit.Objects[noiseconfig.Input];
            if (source is Voltagesource vsource)
            {
                var ac = vsource.Parameters.Get<Components.VoltagesourceBehaviors.FrequencyParameters>();
                if (!ac.VSRCacMag.Given || ac.VSRCacMag == 0.0)
                    throw new CircuitException($"{Name}: Noise input source {vsource.Name} has no AC input");
            }
            else if (source is Currentsource isource)
            {
                var ac = isource.Parameters.Get<Components.CurrentsourceBehaviors.FrequencyParameters>();
                if (!ac.AcMagnitude.Given || ac.AcMagnitude == 0.0)
                    throw new CircuitException($"{Name}: Noise input source {isource.Name} has not AC input");
            }
            else
                throw new CircuitException($"{Name}: No input source");

            double freqdelta = 0.0;
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

            // Initialize
            var data = NoiseState;
            state.Initialize(circuit);
            data.Initialize(freqconfig.StartFreq);
            state.Laplace = 0;
            state.Domain = State.DomainTypes.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Gmin = baseconfig.Gmin;
            Op(baseconfig.DcMaxIterations);
            state.Sparse |= State.SparseFlags.NIACSHOULDREORDER;

            // Connect noise sources
            foreach (var behavior in noisebehaviors)
                behavior.ConnectNoise();

            // Loop through noise figures
            for (int i = 0; i < n; i++)
            {
                state.Laplace = new Complex(0.0, 2.0 * Math.PI * data.Freq);
                AcIterate(circuit);

                double rval = state.Solution[posOutNode] - state.Solution[negOutNode];
                double ival = state.iSolution[posOutNode] - state.iSolution[negOutNode];
                data.GainSqInv = 1.0 / Math.Max(rval * rval + ival * ival, 1e-20);

                // Solve the adjoint system
                NzIterate(posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                data.outNdens = 0.0;
                foreach (var behavior in noisebehaviors)
                    behavior.Noise(this);

                // Export the data
                Export(exportargs);

                // Increment the frequency
                switch (freqconfig.StepType)
                {
                    case StepTypes.Decade:
                    case StepTypes.Octave:
                        data.Freq = data.Freq * freqdelta;
                        break;

                    case StepTypes.Linear:
                        data.Freq = freqconfig.StartFreq + i * freqdelta;
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
        /// <param name="posDrive">The positive driving node</param>
        /// <param name="negDrive">The negative driving node</param>
        void NzIterate(int posDrive, int negDrive)
        {
            var state = State;

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

            state.StoreComplexSolution();

            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;
        }

        /// <summary>
        /// Create an export method for this type of simulation
        /// The simulation will determine which export method is returned if multiple behaviors implement a export method by the same name
        /// </summary>
        /// <param name="name">The identifier of the entity</param>
        /// <param name="property">The parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(Identifier name, string property)
        {
            var eb = pool.GetEntityBehaviors(name) ?? throw new CircuitException($"{Name}: Could not find behaviors of {name}");

            // Most logical place to look for noise analysis: noise behaviors
            var export = eb.Get<NoiseBehavior>()?.CreateExport(property);

            // Next most logical place is the AcBehavior
            if (export == null)
                export = eb.Get<FrequencyBehavior>()?.CreateExport(property);

            // Finally look to the LoadBehavior
            if (export == null)
                export = eb.Get<LoadBehavior>()?.CreateExport(property);
            return export;
        }

        /// <summary>
        /// Create an export for the total input density
        /// </summary>
        /// <param name="input">True if the noise density has to be input-referred</param>
        /// <returns></returns>
        public Func<State, double> CreateNoiseDensityExport(bool input)
        {
            if (input)
                return (State state) => NoiseState.outNdens * NoiseState.GainSqInv;
            return (State state) => NoiseState.outNdens;
        }
    }
}
