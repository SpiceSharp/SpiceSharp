using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    public partial class BiasingBehavior : SubcircuitBehavior<IBiasingBehavior>, IBiasingBehavior, IConvergenceBehavior
    {
        private readonly BehaviorList<IConvergenceBehavior> _convergenceBehaviors;

        /// <summary>
        /// Prepares the specified simulation for biasing behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(SubcircuitSimulation simulation)
        {
            var parameters = simulation.GetParameterSet<BaseParameters>();
            if (simulation.UsesState<IBiasingSimulationState>())
            {
                var parent = simulation.GetState<IBiasingSimulationState>();
                IBiasingSimulationState state;
                if (parameters.LocalBiasingSolver && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                    state = new LocalSimulationState(simulation.InstanceName, parent, simulation.Nodes, LUHelper.CreateSparseRealSolver());
                else
                    state = new FlatSimulationState(simulation.InstanceName, parent, simulation.Nodes);
                simulation.LocalStates.Add(state);
            }
        }
        private readonly LocalSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public BiasingBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalStates.TryGetValue(out _state))
                _state.Initialize(simulation.Nodes);
            _convergenceBehaviors = simulation.EntityBehaviors.GetBehaviorList<IConvergenceBehavior>();
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            if (_state != null)
            {
                _state.Update();
                do
                {
                    _state.Solver.Reset();
                    LoadBehaviors();
                }
                while (!_state.Apply());
            }
            else
                LoadBehaviors();
        }

        /// <summary>
        /// Loads the behaviors.
        /// </summary>
        protected virtual void LoadBehaviors()
        {
            foreach (var behavior in Behaviors)
                behavior.Load();
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            _state?.Update();
            var result = true;
            foreach (var behavior in _convergenceBehaviors)
                result &= behavior.IsConvergent();
            return result;
        }
    }
}
