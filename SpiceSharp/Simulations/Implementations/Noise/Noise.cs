using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that implements a noise analysis.
    /// </summary>
    /// <seealso cref="FrequencySimulation" />
    public partial class Noise : FrequencySimulation,
        INoiseSimulation,
        IParameterized<NoiseParameters>
    {
        private NoiseSimulationState _state;
        private BehaviorList<INoiseBehavior> _noiseBehaviors;

        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        /// <value>
        /// The noise parameters.
        /// </value>
        public NoiseParameters NoiseParameters { get; } = new NoiseParameters();

        /// <inheritdoc/>
        NoiseParameters IParameterized<NoiseParameters>.Parameters => NoiseParameters;

        /// <inheritdoc/>
        INoiseSimulationState IStateful<INoiseSimulationState>.State => _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Noise(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="input">The name of the input source.</param>
        /// <param name="output">The output node name.</param>
        /// <param name="frequencySweep">The frequency points.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Noise(string name, string input, string output, IEnumerable<double> frequencySweep)
            : base(name, frequencySweep)
        {
            NoiseParameters.InputSource = input;
            NoiseParameters.Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="input">The name of the input source.</param>
        /// <param name="output">The output node name.</param>
        /// <param name="reference">The reference output node name.</param>
        /// <param name="frequencySweep">The frequency points.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Noise(string name, string input, string output, string reference, IEnumerable<double> frequencySweep)
            : base(name, frequencySweep)
        {
            NoiseParameters.InputSource = input;
            NoiseParameters.Output = output;
            NoiseParameters.OutputRef = reference;
        }

        /// <inheritdoc />
        protected override void CreateStates()
        {
            base.CreateStates();
            _state = new NoiseSimulationState(Name);
        }

        /// <inheritdoc />
        protected override void CreateBehaviors(IEntityCollection entities)
        {
            base.CreateBehaviors(entities);
            _noiseBehaviors = EntityBehaviors.GetBehaviorList<INoiseBehavior>();
        }

        /// <inheritdoc/>
        protected override void Execute()
        {
            base.Execute();
            var cstate = (ComplexSimulationState)GetState<IComplexSimulationState>();
            var noiseconfig = NoiseParameters;
            var exportargs = new ExportDataEventArgs(this);

            // Find the output nodes
            var posOutNode = noiseconfig.Output != null ? cstate.Map[cstate.GetSharedVariable(noiseconfig.Output)] : 0;
            var negOutNode = noiseconfig.OutputRef != null ? cstate.Map[cstate.GetSharedVariable(noiseconfig.OutputRef)] : 0;

            // We only want to enable the source that is flagged as the input
            var source = EntityBehaviors[NoiseParameters.InputSource];
            var originalParameters = new List<Tuple<IndependentSourceParameters, double, double>>();
            foreach (var container in EntityBehaviors)
            {
                if (container.TryGetParameterSet(out IndependentSourceParameters parameters))
                {
                    originalParameters.Add(Tuple.Create(parameters, parameters.AcMagnitude, parameters.AcPhase));
                    if (ReferenceEquals(container, source))
                    {
                        parameters.AcMagnitude = 1.0;
                        parameters.AcPhase = 0.0;
                    }
                    else
                    {
                        parameters.AcMagnitude = 0.0;
                        parameters.AcPhase = 0.0;
                    }
                }
            }

            try
            {
                // Initialize
                var freq = FrequencyParameters.Frequencies.GetEnumerator();
                if (!freq.MoveNext())
                    return;
                cstate.Laplace = 0;
                Op(BiasingParameters.DcMaxIterations);

                // Initialize all devices for small-signal analysis and reset all noise contributions
                InitializeAcParameters();
                foreach (var behavior in _noiseBehaviors)
                    behavior.Initialize();

                // Loop through noise figures
                do
                {
                    // First compute the AC gain
                    cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq.Current);
                    AcIterate();

                    var val = cstate.Solution[posOutNode] - cstate.Solution[negOutNode];
                    var inverseGainSquared = 1.0 / Math.Max(val.Real * val.Real + val.Imaginary * val.Imaginary, 1e-20);
                    _state.SetCurrentPoint(new NoisePoint(freq.Current, inverseGainSquared));

                    // Solve the adjoint system
                    NzIterate(posOutNode, negOutNode);

                    // Now we use the adjoint system to calculate the noise
                    // contributions of each generator in the circuit
                    foreach (var behavior in _noiseBehaviors)
                    {
                        behavior.Compute();
                        _state.Add(behavior);
                    }

                    // Export the data
                    OnExport(exportargs);
                }
                while (freq.MoveNext());
            }
            finally
            {
                foreach (var parameters in originalParameters)
                {
                    parameters.Item1.AcMagnitude = parameters.Item2;
                    parameters.Item1.AcPhase = parameters.Item3;
                }
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
            var cstate = GetState<IComplexSimulationState>();
            var solver = cstate.Solver;

            // Clear out the right hand side vector
            solver.Precondition((matrix, rhs) =>
            {
                rhs.Reset();
            });

            // Apply unit current excitation
            solver.GetElement(posDrive).Add(1.0);
            solver.GetElement(negDrive).Subtract(1.0);

            solver.SolveTransposed(cstate.Solution);
            cstate.Solution[0] = 0.0;
        }
    }
}
