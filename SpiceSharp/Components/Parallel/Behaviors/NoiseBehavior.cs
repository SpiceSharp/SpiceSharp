using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="INoiseBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="INoiseBehavior" />
    public partial class NoiseBehavior : Behavior, INoiseBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.NoiseDistributor != null)
            {
                if (simulation.UsesState<INoiseSimulationState>())
                {
                    var state = simulation.GetParentState<INoiseSimulationState>();
                    simulation.LocalStates.Add(new NoiseSimulationState(state));
                }
            }
        }

        private readonly Workload _noiseWorkload;
        private readonly BehaviorList<INoiseBehavior> _noiseBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public NoiseBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            _noiseBehaviors = simulation.EntityBehaviors.GetBehaviorList<INoiseBehavior>();
            if (parameters.NoiseDistributor != null)
            {
                _noiseWorkload = new Workload(parameters.NoiseDistributor, _noiseBehaviors.Count);
                foreach (var behavior in _noiseBehaviors)
                    _noiseWorkload.Actions.Add(behavior.Noise);
            }
        }

        void INoiseBehavior.Noise()
        {
            if (_noiseWorkload != null)
                _noiseWorkload.Execute();
            else
            {
                foreach (var behavior in _noiseBehaviors)
                    behavior.Noise();
            }
        }
    }
}
