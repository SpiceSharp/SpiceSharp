using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public partial class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Prepares the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(ParallelSimulation simulation)
        {
            if (!simulation.UsesState<IBiasingSimulationState>())
                return;
            if (simulation.LocalConfigurations.TryGetValue<BiasingParameters>(out var result))
            {
                if (result.LoadDistributor != null && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                {
                    var state = simulation.GetParentState<IBiasingSimulationState>();
                    simulation.LocalStates.Add(new SimulationState(state));
                }
            }
        }

        private readonly SimulationState _state;
        private readonly Workload _load;
        private readonly Workload<bool> _convergence;
        private readonly BehaviorList<IBiasingBehavior> _biasingBehaviors;
        private readonly BehaviorList<IConvergenceBehavior> _convergenceBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        public BiasingBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            if (simulation.LocalConfigurations.TryGetValue<BiasingParameters>(out var result))
            {
                _state = simulation.GetState<SimulationState>();
                if (result.LoadDistributor != null)
                    _load = new Workload(result.LoadDistributor, simulation.EntityBehaviors.Count);
                if (result.ConvergenceDistributor != null)
                    _convergence = new Workload<bool>(result.ConvergenceDistributor, simulation.EntityBehaviors.Count);
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out IBiasingBehavior biasing))
                        _load?.Actions.Add(biasing.Load);
                    if (container.TryGetValue(out IConvergenceBehavior convergence))
                        _convergence?.Functions.Add(convergence.IsConvergent);
                }
            }
            else
            {
                _convergence = null;
                _load = null;
                _state = null;
            }

            // Get all behaviors
            _biasingBehaviors = simulation.EntityBehaviors.GetBehaviorList<IBiasingBehavior>();
            _convergenceBehaviors = simulation.EntityBehaviors.GetBehaviorList<IConvergenceBehavior>();
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            if (_convergence != null)
                return _convergence.Execute();
            else
            {
                var convergence = true;
                foreach (var behavior in _convergenceBehaviors)
                    convergence &= behavior.IsConvergent();
                return convergence;
            }
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public virtual void Load()
        {
            if (_load != null)
            {
                _state.Reset();
                _load.Execute();
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
