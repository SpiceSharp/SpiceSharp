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
        /// Get the noise state
        /// </summary>
        public StateNoise NoiseState { get; } = new StateNoise();

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
            Parameters.Add(new NoiseConfiguration());
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
            Parameters.Add(new NoiseConfiguration(output, null, input));
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
            NoiseBehaviors = SetupBehaviors<NoiseBehavior>();
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
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

            var circuit = Circuit;
            var state = State;
            var noiseconfig = NoiseConfiguration;
            var baseconfig = BaseConfiguration;
            var exportargs = new ExportDataEventArgs(State);

            // Find the output nodes
            int posOutNode = noiseconfig.Output != null ? circuit.Nodes[noiseconfig.Output].Index : 0;
            int negOutNode = noiseconfig.OutputRef != null ? circuit.Nodes[noiseconfig.OutputRef].Index : 0;

            // Check the voltage or current source
            // TODO: Note used? Check this!
            // var source = FindInputSource(noiseconfig.Input);
            
            // Initialize
            var data = NoiseState;
            state.Initialize(circuit);
            data.Initialize(FrequencySweep.Initial);
            state.Laplace = 0;
            state.Domain = State.DomainType.Frequency;
            state.UseIC = false;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Gmin = baseconfig.Gmin;
            Op(baseconfig.DCMaxIterations);
            state.Sparse |= State.SparseStates.ACShouldReorder;

            // Connect noise sources
            foreach (var behavior in NoiseBehaviors)
                behavior.ConnectNoise();

            // Loop through noise figures
            foreach (double freq in FrequencySweep.Points)
            {
                data.Frequency = freq;
                state.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);
                ACIterate(circuit);

                Complex val = state.ComplexSolution[posOutNode] - state.ComplexSolution[negOutNode];
                data.GainInverseSquared = 1.0 / Math.Max(val.Real * val.Real + val.Imaginary * val.Imaginary, 1e-20);

                // Solve the adjoint system
                NzIterate(posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                data.OutputNoiseDensity = 0.0;
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
                var ac = vsource.Parameters.Get<Components.VoltagesourceBehaviors.FrequencyParameters>();
                if (!ac.ACMagnitude.Given || ac.ACMagnitude == 0.0)
                    throw new CircuitException("{0}: Noise input source {1} has no AC input".FormatString(Name, vsource.Name));
            }
            else if (source is CurrentSource isource)
            {
                var ac = isource.Parameters.Get<Components.CurrentsourceBehaviors.FrequencyParameters>();
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
            var state = State;

            // Clear out the right hand side vector
            for (int i = 0; i < state.Rhs.Length; i++)
                state.ComplexRhs[i] = 0.0;

            // Apply unit current excitation
            state.ComplexRhs[posDrive] = 1.0;
            state.ComplexRhs[negDrive] = -1.0;

            state.Matrix.SolveTransposed(state.ComplexRhs, state.ComplexRhs);

            state.StoreComplexSolution();

            state.ComplexSolution[0] = 0.0;
        }

        /// <summary>
        /// Create an export method for this type of simulation
        /// The simulation will determine which export method is returned if multiple behaviors implement a export method by the same name
        /// </summary>
        /// <param name="name">The identifier of the entity</param>
        /// <param name="propertyName">The parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(Identifier name, string propertyName)
        {
            var eb = Pool.GetEntityBehaviors(name) ?? throw new CircuitException("{0}: Could not find behaviors of {1}".FormatString(Name, name));

            // Most logical place to look for noise analysis: noise behaviors
            var export = eb.Get<NoiseBehavior>()?.CreateExport(propertyName);

            // Next most logical place is the AcBehavior
            if (export == null)
                export = eb.Get<FrequencyBehavior>()?.CreateExport(propertyName);

            // Finally look to the LoadBehavior
            if (export == null)
                export = eb.Get<LoadBehavior>()?.CreateExport(propertyName);
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
                return (State state) => NoiseState.OutputNoiseDensity * NoiseState.GainInverseSquared;
            return (State state) => NoiseState.OutputNoiseDensity;
        }
    }
}
