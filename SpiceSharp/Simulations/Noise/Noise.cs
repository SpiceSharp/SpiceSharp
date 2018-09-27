using System;
using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that implements a noise analysis.
    /// </summary>
    /// <seealso cref="FrequencySimulation" />
    public class Noise : FrequencySimulation
    {
        /// <summary>
        /// Gets the currently active noise configuration.
        /// </summary>
        /// <value>
        /// The noise configuration.
        /// </value>
        public NoiseConfiguration NoiseConfiguration { get; protected set; }

        /// <summary>
        /// Gets the noise state.
        /// </summary>
        /// <value>
        /// The noise state.
        /// </value>
        public NoiseState NoiseState { get; private set; }

        /// <summary>
        /// Noise behaviors
        /// </summary>
        private BehaviorList<BaseNoiseBehavior> _noiseBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        public Noise(Identifier name) : base(name)
        {
            ParameterSets.Add(new NoiseConfiguration());
            States.Add(new NoiseState());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="output">The output node identifier.</param>
        /// <param name="input">The input source identifier.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public Noise(Identifier name, Identifier output, Identifier input, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
            ParameterSets.Add(new NoiseConfiguration(output, null, input));
            States.Add(new NoiseState());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="output">The output node identifier.</param>
        /// <param name="reference">The reference output node identifier.</param>
        /// <param name="input">The input source identifier.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public Noise(Identifier name, Identifier output, Identifier reference, Identifier input, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
            ParameterSets.Add(new NoiseConfiguration(output, reference, input));
            States.Add(new NoiseState());
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Get behaviors, parameters and states
            NoiseConfiguration = ParameterSets.Get<NoiseConfiguration>();
            _noiseBehaviors = SetupBehaviors<BaseNoiseBehavior>(circuit.Objects);
            NoiseState = States.Get<NoiseState>();
            NoiseState.Setup(Nodes);
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            NoiseState.Destroy();
            NoiseState = null;
            for (var i = 0; i < _noiseBehaviors.Count; i++)
                _noiseBehaviors[i].Unsetup(this);
            _noiseBehaviors = null;
            NoiseConfiguration = null;

            base.Unsetup();
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            var state = RealState;
            var cstate = ComplexState;
            var nstate = NoiseState;

            var noiseconfig = NoiseConfiguration;
            var baseconfig = BaseConfiguration;
            var exportargs = new ExportDataEventArgs(this);

            // Find the output nodes
            var posOutNode = noiseconfig.Output != null ? Nodes.GetNode(noiseconfig.Output).Index : 0;
            var negOutNode = noiseconfig.OutputRef != null ? Nodes.GetNode(noiseconfig.OutputRef).Index : 0;

            // Initialize
            nstate.Reset(FrequencySweep.Initial);
            cstate.Laplace = 0;
            state.Domain = RealState.DomainType.None;
            state.UseIc = false;
            state.UseDc = true;
            state.Gmin = BaseConfiguration.Gmin;
            Op(baseconfig.DcMaxIterations);
            state.Sparse |= RealState.SparseStates.AcShouldReorder;

            // Load all in order to calculate the AC info for all devices
            InitializeAcParameters();

            // Connect noise sources
            for (var i = 0; i < _noiseBehaviors.Count; i++)
                _noiseBehaviors[i].ConnectNoise();

            // Loop through noise figures
            foreach (var freq in FrequencySweep.Points)
            {
                nstate.Frequency = freq;
                cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);
                AcIterate();

                var val = cstate.Solution[posOutNode] - cstate.Solution[negOutNode];
                nstate.GainInverseSquared = 1.0 / Math.Max(val.Real * val.Real + val.Imaginary * val.Imaginary, 1e-20);

                // Solve the adjoint system
                NzIterate(posOutNode, negOutNode);

                // Now we use the adjoint system to calculate the noise
                // contributions of each generator in the circuit
                nstate.OutputNoiseDensity = 0.0;
                for (var i = 0; i < _noiseBehaviors.Count; i++)
                    _noiseBehaviors[i].Noise(this);

                // Export the data
                OnExport(exportargs);
            }
        }

        /// <summary>
        /// Calculate the solution for <see cref="Noise" /> analysis
        /// </summary>
        /// <param name="posDrive">The positive driving node index.</param>
        /// <param name="negDrive">The negative driving node index.</param>
        /// <remarks>
        /// This routine solves the adjoint system. It assumes that the matrix has
        /// already been loaded by a call to AcIterate, so it only alters the right
        /// hand side vector. The unit-valued current excitation is applied between
        /// nodes posDrive and negDrive.
        /// </remarks>
        private void NzIterate(int posDrive, int negDrive)
        {
            var solver = ComplexState.Solver;

            // Clear out the right hand side vector
            var element = solver.FirstInReorderedRhs();
            while (element != null)
            {
                element.Value = 0.0;
                element = element.Below;
            }

            // Apply unit current excitation
            solver.GetRhsElement(posDrive).Value = 1.0;
            solver.GetRhsElement(negDrive).Value = -1.0;

            solver.SolveTransposed(ComplexState.Solution);
            ComplexState.Solution[0] = 0.0;
        }
    }
}
