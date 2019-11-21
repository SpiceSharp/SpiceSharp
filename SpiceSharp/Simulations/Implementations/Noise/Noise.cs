using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that implements a noise analysis.
    /// </summary>
    /// <seealso cref="FrequencySimulation" />
    public partial class Noise : FrequencySimulation,
        INoiseSimulation
    {
        /// <summary>
        /// Gets the currently active noise configuration.
        /// </summary>
        public NoiseConfiguration NoiseConfiguration { get; protected set; }

        /// <summary>
        /// Gets the noise simulation state.
        /// </summary>
        protected NoiseSimulationState NoiseState { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public new INoiseSimulationState State => NoiseState;

        /// <summary>
        /// Noise behaviors
        /// </summary>
        private BehaviorList<INoiseBehavior> _noiseBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        public Noise(string name) : base(name)
        {
            Configurations.Add(new NoiseConfiguration());
            NoiseState = new NoiseSimulationState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="output">The output node identifier.</param>
        /// <param name="input">The input source identifier.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public Noise(string name, string output, string input, Sweep<double> frequencySweep) 
            : base(name, frequencySweep)
        {
            Configurations.Add(new NoiseConfiguration(output, null, input));
            NoiseState = new NoiseSimulationState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="output">The output node identifier.</param>
        /// <param name="reference">The reference output node identifier.</param>
        /// <param name="input">The input source identifier.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public Noise(string name, string output, string reference, string input, Sweep<double> frequencySweep) 
            : base(name, frequencySweep)
        {
            Configurations.Add(new NoiseConfiguration(output, reference, input));
            NoiseState = new NoiseSimulationState();
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="state">The state.</param>
        public void GetState(out INoiseSimulationState state) => state = NoiseState;

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Get behaviors, parameters and states
            NoiseConfiguration = Configurations.GetValue<NoiseConfiguration>();
            NoiseState.Setup(this);
            base.Setup(entities);

            // Cache local variables
            _noiseBehaviors = EntityBehaviors.GetBehaviorList<INoiseBehavior>();
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            NoiseState.Unsetup();
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

            var state = BiasingState;
            var cstate = ComplexState;
            var nstate = NoiseState;

            var noiseconfig = NoiseConfiguration;
            var exportargs = new ExportDataEventArgs(this);

            // Find the output nodes
            var posOutNode = noiseconfig.Output != null ? state.Map[Variables[noiseconfig.Output]] : 0;
            var negOutNode = noiseconfig.OutputRef != null ? state.Map[Variables[noiseconfig.OutputRef]] : 0;

            // Initialize
            nstate.Reset(FrequencySweep.Initial);
            cstate.Laplace = 0;
            state.UseIc = false;
            state.UseDc = true;
            Op(DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            InitializeAcParameters();

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
                foreach (var behavior in _noiseBehaviors)
                    behavior.Noise();

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
            solver.ResetVector();

            // Apply unit current excitation
            solver.GetElement(posDrive).Add(1.0);
            solver.GetElement(negDrive).Subtract(1.0);

            solver.SolveTransposed(ComplexState.Solution);
            ComplexState.Solution[0] = 0.0;
        }
    }
}
