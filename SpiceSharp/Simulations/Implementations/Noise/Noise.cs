using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
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
        /// The constant returned when exporting the operating point.
        /// </summary>
        public const int ExportOperatingPoint = 0x01;

        /// <summary>
        /// The constant returned when exporting the small signal solution.
        /// </summary>
        public const int ExportSmallSignal = 0x02;

        /// <summary>
        /// The constant returned when exporting noise.
        /// </summary>
        public const int ExportNoise = 0x04;

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
        /// Gets the current frequency point.
        /// </summary>
        public double Frequency => GetState<IComplexSimulationState>().Laplace.Imaginary / (2 * Math.PI);

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
        protected override IEnumerable<int> Execute(int mask = Exports)
        {
            foreach (int exportType in base.Execute(mask))
                yield return exportType;

            var cstate = (ComplexSimulationState)GetState<IComplexSimulationState>();
            var noiseconfig = NoiseParameters;

            // Find the output nodes
            int posOutNode = noiseconfig.Output != null ? cstate.Map[cstate.GetSharedVariable(noiseconfig.Output)] : 0;
            int negOutNode = noiseconfig.OutputRef != null ? cstate.Map[cstate.GetSharedVariable(noiseconfig.OutputRef)] : 0;

            // We only want to enable the source that is flagged as the input
            var originalState = new List<Tuple<IndependentSourceParameters, double, double>>();
            CollectSourceStates(EntityBehaviors, originalState);

            // Drive the input
            var source = EntityBehaviors[NoiseParameters.InputSource].GetParameterSet<IndependentSourceParameters>();
            source.AcMagnitude = 1.0;
            source.AcPhase = 0.0;
            
            try
            {
                // Initialize
                var freq = FrequencyParameters.Frequencies.GetEnumerator();
                if (!freq.MoveNext())
                    yield break;
                cstate.Laplace = 0;
                Op(BiasingParameters.DcMaxIterations);

                // Initialize all devices for small-signal analysis and reset all noise contributions
                InitializeAcParameters();
                foreach (var behavior in _noiseBehaviors)
                    behavior.Initialize();

                // Export the operating point
                if ((mask & ExportOperatingPoint) != 0)
                    yield return ExportOperatingPoint;

                // Loop through noise figures
                do
                {
                    // First compute the AC gain
                    cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq.Current);
                    AcIterate();

                    var val = cstate.Solution[posOutNode] - cstate.Solution[negOutNode];
                    double inverseGainSquared = 1.0 / Math.Max(val.Real * val.Real + val.Imaginary * val.Imaginary, 1e-20);
                    _state.SetCurrentPoint(new NoisePoint(freq.Current, inverseGainSquared));

                    // Export AC solution
                    if ((mask & ExportSmallSignal) != 0)
                        yield return ExportSmallSignal;

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
                    if ((mask & ExportNoise) != 0)
                        yield return ExportNoise;
                }
                while (freq.MoveNext());
            }
            finally
            {
                RestoreSourceStates(originalState);
            }
        }

        private void CollectSourceStates(IBehaviorContainerCollection behaviorContainers, List<Tuple<IndependentSourceParameters, double, double>> state)
        {
            foreach (var behaviors in behaviorContainers)
            {
                if (behaviors.TryGetParameterSet(out IndependentSourceParameters parameters))
                {
                    state.Add(Tuple.Create(parameters, parameters.AcMagnitude, parameters.AcPhase));
                    parameters.AcMagnitude = 0.0;
                    parameters.AcPhase = 0.0;
                }
                if (behaviors.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior))
                    CollectSourceStates(entitiesBehavior.LocalBehaviors, state);
            }
        }

        private void RestoreSourceStates(List<Tuple<IndependentSourceParameters, double, double>> state)
        {
            foreach (var parameters in state)
            {
                parameters.Item1.AcMagnitude = parameters.Item2;
                parameters.Item1.AcPhase = parameters.Item3;
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

            // Prepare all noise sources for the coming
            foreach (var noise in _noiseBehaviors)
                noise.Load();

            // Apply unit current excitation
            solver.GetElement(posDrive).Add(1.0);
            solver.GetElement(negDrive).Subtract(1.0);

            solver.ForwardSubstituteTransposed(cstate.Solution);
            solver.BackwardSubstituteTransposed(cstate.Solution);
            cstate.Solution[0] = 0.0;
        }
    }
}
