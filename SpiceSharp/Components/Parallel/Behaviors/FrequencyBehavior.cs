using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior" /> for a <see cref="ParallelComponents" />.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IFrequencyBehavior" />
    public partial class FrequencyBehavior : Behavior, IFrequencyBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            if (!simulation.UsesState<IComplexSimulationState>())
                return;
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.AcLoadDistributor != null && !simulation.LocalStates.ContainsKey(typeof(IComplexSimulationState)))
            {
                var state = simulation.GetParentState<IComplexSimulationState>();
                simulation.LocalStates.Add(new ComplexSimulationState(state));
            }
        }

        private readonly ComplexSimulationState _state;
        private readonly Workload _loadWorkload, _initWorkload;
        private readonly BehaviorList<IFrequencyBehavior> _frequencyBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public FrequencyBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            simulation.TryGetState(out _state);
            if (parameters.AcLoadDistributor != null)
                _loadWorkload = new Workload(parameters.AcLoadDistributor, simulation.EntityBehaviors.Count);
            if (parameters.AcInitDistributor != null)
                _initWorkload = new Workload(parameters.AcInitDistributor, simulation.EntityBehaviors.Count);
            if (_loadWorkload != null || _initWorkload != null)
            {
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out IFrequencyBehavior behavior))
                    {
                        _loadWorkload?.Actions.Add(behavior.Load);
                        _initWorkload?.Actions.Add(behavior.InitializeParameters);
                    }
                }
            }

            // Get all behaviors
            _frequencyBehaviors = simulation.EntityBehaviors.GetBehaviorList<IFrequencyBehavior>();
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            if (_initWorkload != null)
                _initWorkload.Execute();
            else
            {
                foreach (var behavior in _frequencyBehaviors)
                    behavior.InitializeParameters();
            }
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            if (_loadWorkload != null)
            {
                _state.Reset();
                _loadWorkload.Execute();
                _state.Apply();
            }
            else
            {
                foreach (var behavior in _frequencyBehaviors)
                    behavior.Load();
            }
        }
    }
}
