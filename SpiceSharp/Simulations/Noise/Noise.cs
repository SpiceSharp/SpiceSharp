using System;
using System.Collections.ObjectModel;
using System.Numerics;
using SpiceSharp.Diagnostics;
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
        /// Gets the noise state
        /// </summary>
        public NoiseState NoiseState { get; private set; }

        /// <summary>
        /// Noise behaviors
        /// </summary>
        protected Collection<NoiseBehavior> NoiseBehaviors { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public Noise(Identifier name) : base(name)
        {
            ParameterSets.Add(new NoiseConfiguration());
            States.Add(new NoiseState());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="output">Output node</param>
        /// <param name="input">Input source</param>
        /// <param name="frequencySweep">Frequency sweep</param>
        public Noise(Identifier name, Identifier output, Identifier input, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
            ParameterSets.Add(new NoiseConfiguration(output, null, input));
            States.Add(new NoiseState());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="output">Output node</param>
        /// <param name="reference">Reference output node</param>
        /// <param name="input">Input</param>
        /// <param name="frequencySweep">Frequency sweep</param>
        public Noise(Identifier name, Identifier output, Identifier reference, Identifier input, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
            ParameterSets.Add(new NoiseConfiguration(output, reference, input));
            States.Add(new NoiseState());
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors, parameters and states
            NoiseConfiguration = ParameterSets.Get<NoiseConfiguration>();
            NoiseBehaviors = SetupBehaviors<NoiseBehavior>();
            NoiseState = States.Get<NoiseState>();
            NoiseState.Initialize(Circuit.Nodes);
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            NoiseState.Destroy();
            NoiseState = null;
            foreach (var behavior in NoiseBehaviors)
                behavior.Unsetup();
            NoiseBehaviors.Clear();
            NoiseBehaviors = null;
            NoiseConfiguration = null;

            base.Unsetup();
        }

        /// <summary>
        /// Execute the noise analysis
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            var state = RealState;
            var cstate = ComplexState;
            var nstate = NoiseState;

            var noiseconfig = NoiseConfiguration;
            var baseconfig = BaseConfiguration;
            var exportargs = new ExportDataEventArgs(RealState, ComplexState);

            // Find the output nodes
            int posOutNode = noiseconfig.Output != null ? Circuit.Nodes[noiseconfig.Output].Index : 0;
            int negOutNode = noiseconfig.OutputRef != null ? Circuit.Nodes[noiseconfig.OutputRef].Index : 0;

            // Check the voltage or current source
            // TODO: Note used? Check this!
            // var source = FindInputSource(noiseconfig.Input);
            
            // Initialize
            nstate.Reset(FrequencySweep.Initial);
            cstate.Laplace = 0;
            state.Domain = RealState.DomainType.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            Op(baseconfig.DCMaxIterations);
            state.Sparse |= RealState.SparseStates.ACShouldReorder;

            // Connect noise sources
            foreach (var behavior in NoiseBehaviors)
                behavior.ConnectNoise();

            // Loop through noise figures
            foreach (double freq in FrequencySweep.Points)
            {
                nstate.Frequency = freq;
                cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);
                ACIterate();

                Complex val = cstate.Solution[posOutNode] - cstate.Solution[negOutNode];
                nstate.GainInverseSquared = 1.0 / Math.Max(val.Real * val.Real + val.Imaginary * val.Imaginary, 1e-20);

                // Solve the adjoint system
                NzIterate(posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                nstate.OutputNoiseDensity = 0.0;
                foreach (var behavior in NoiseBehaviors)
                    behavior.Noise(this);

                // Export the data
                Export(exportargs);
            }
        }

        /// <summary>
        /// Find the input source used for calculating the input noise
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        Entity FindInputSource(Identifier name)
        {
            if (name == null)
                throw new CircuitException("{0}: No input source specified".FormatString(Name));

            Entity source = Circuit.Objects[name];
            if (source is VoltageSource vsource)
            {
                var ac = vsource.ParameterSets.Get<Components.VoltagesourceBehaviors.FrequencyParameters>();
                if (!ac.ACMagnitude.Given || ac.ACMagnitude == 0.0)
                    throw new CircuitException("{0}: Noise input source {1} has no AC input".FormatString(Name, vsource.Name));
            }
            else if (source is CurrentSource isource)
            {
                var ac = isource.ParameterSets.Get<Components.CurrentsourceBehaviors.FrequencyParameters>();
                if (!ac.ACMagnitude.Given || ac.ACMagnitude == 0.0)
                    throw new CircuitException("{0}: Noise input source {1} has not AC input".FormatString(Name, isource.Name));
            }
            else
                throw new CircuitException("{0}: No input source".FormatString(Name));

            return source;
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
            var state = ComplexState;

            // Clear out the right hand side vector
            for (int i = 0; i < state.Rhs.Length; i++)
                state.Rhs[i] = 0.0;

            // Apply unit current excitation
            state.Rhs[posDrive] = 1.0;
            state.Rhs[negDrive] = -1.0;

            state.Matrix.SolveTransposed(state.Rhs, state.Rhs);
            state.StoreSolution();
            state.Solution[0] = 0.0;
        }

        /// <summary>
        /// Create an export for the total input density
        /// </summary>
        /// <param name="input">True if the noise density has to be input-referred</param>
        /// <returns></returns>
        public Func<RealState, double> CreateNoiseDensityExport(bool input)
        {
            if (input)
                return (RealState state) => NoiseState.OutputNoiseDensity * NoiseState.GainInverseSquared;
            return (RealState state) => NoiseState.OutputNoiseDensity;
        }
    }
}
