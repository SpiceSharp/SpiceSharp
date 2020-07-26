using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Linq;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="INoiseBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="INoiseBehavior" />
    public partial class Noise : Behavior,
        INoiseBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            if (parameters.NoiseComputeDistributor != null)
            {
                if (simulation.UsesState<INoiseSimulationState>())
                {
                    var state = simulation.GetParentState<INoiseSimulationState>();
                    simulation.LocalStates.Add<NoiseSimulationState>(new NoiseSimulationState(state));
                }
            }
        }

        private readonly Workload _noiseInitializeWorkload, _noiseComputeWorkload;
        private readonly BehaviorList<INoiseBehavior> _noiseBehaviors;

        /// <inheritdoc/>
        public double OutputNoiseDensity => _noiseBehaviors.Sum(nb => nb.OutputNoiseDensity);

        /// <inheritdoc/>
        public double TotalOutputNoise => _noiseBehaviors.Sum(nb => nb.TotalOutputNoise);

        /// <inheritdoc/>
        public double TotalInputNoise => _noiseBehaviors.Sum(nb => nb.TotalInputNoise);

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public Noise(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            _noiseBehaviors = simulation.EntityBehaviors.GetBehaviorList<INoiseBehavior>();
            if (parameters.NoiseComputeDistributor != null)
            {
                _noiseComputeWorkload = new Workload(parameters.NoiseComputeDistributor, _noiseBehaviors.Count);
                foreach (var behavior in _noiseBehaviors)
                    _noiseComputeWorkload.Actions.Add(behavior.Compute);
            }
            if (parameters.NoiseInitializeDistributor != null)
            {
                _noiseInitializeWorkload = new Workload(parameters.NoiseInitializeDistributor, _noiseBehaviors.Count);
                foreach (var behavior in _noiseBehaviors)
                    _noiseInitializeWorkload.Actions.Add(behavior.Initialize);
            }
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            if (_noiseInitializeWorkload != null)
                _noiseInitializeWorkload.Execute();
            else
            {
                foreach (var behavior in _noiseBehaviors)
                    behavior.Initialize();
            }
        }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            if (_noiseComputeWorkload != null)
                _noiseComputeWorkload.Execute();
            else
            {
                foreach (var behavior in _noiseBehaviors)
                    behavior.Compute();
            }
        }
    }
}
