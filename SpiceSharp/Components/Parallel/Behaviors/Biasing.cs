using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public partial class Biasing : Behavior,
        IBiasingBehavior
    {
        private readonly BiasingSimulationState _state;
        private readonly Workload _loadWorkload;
        private readonly BehaviorList<IBiasingBehavior> _biasingBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public Biasing(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<Parameters>();
            simulation.TryGetState(out _state);
            if (parameters.BiasLoadDistributor != null)
            {
                _loadWorkload = new Workload(parameters.BiasLoadDistributor, simulation.EntityBehaviors.Count);
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out IBiasingBehavior biasing))
                        _loadWorkload.Actions.Add(biasing.Load);
                }
            }

            // Get all behaviors
            _biasingBehaviors = simulation.EntityBehaviors.GetBehaviorList<IBiasingBehavior>();
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_loadWorkload != null)
            {
                _state.Reset();
                _loadWorkload.Execute();
                _state.Apply();
            }
            else
            {
                foreach (var behavior in _biasingBehaviors)
                    behavior.Load();
            }
        }
    }
}
