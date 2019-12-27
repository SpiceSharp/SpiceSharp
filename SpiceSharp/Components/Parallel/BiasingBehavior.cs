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
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            if (parameters.LoadDistributor != null && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
            {
                var state = simulation.GetParentState<IBiasingSimulationState>();
                simulation.LocalStates.Add(new SimulationState(state));
            }
        }

        private readonly SimulationState _state;
        private readonly Workload _loadWorkload;
        private readonly Workload<bool> _convergenceWorkload;
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
            var parameters = simulation.LocalParameters.GetParameterSet<BaseParameters>();
            _state = simulation.GetState<SimulationState>();
            if (parameters.LoadDistributor != null)
                _loadWorkload = new Workload(parameters.LoadDistributor, simulation.EntityBehaviors.Count);
            if (parameters.ConvergenceDistributor != null)
                _convergenceWorkload = new Workload<bool>(parameters.ConvergenceDistributor, simulation.EntityBehaviors.Count);
            if (_loadWorkload != null || _convergenceWorkload != null)
            {
                foreach (var container in simulation.EntityBehaviors)
                {
                    if (container.TryGetValue(out IBiasingBehavior biasing))
                        _loadWorkload?.Actions.Add(biasing.Load);
                    if (container.TryGetValue(out IConvergenceBehavior convergence))
                        _convergenceWorkload?.Functions.Add(convergence.IsConvergent);
                }
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
            if (_convergenceWorkload != null)
                return _convergenceWorkload.Execute();
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
