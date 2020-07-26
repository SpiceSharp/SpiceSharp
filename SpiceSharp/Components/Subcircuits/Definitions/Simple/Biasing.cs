using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IConvergenceBehavior"/>
    public partial class Biasing : SubcircuitBehavior<IBiasingBehavior>,
        IBiasingBehavior,
        IConvergenceBehavior
    {
        private readonly BehaviorList<IConvergenceBehavior> _convergenceBehaviors;

        /// <summary>
        /// Prepares the specified simulation for biasing behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(SubcircuitSimulation simulation)
        {
            var parameters = simulation.GetParameterSet<Parameters>();
            if (simulation.UsesState<IBiasingSimulationState>())
            {
                var parent = simulation.GetState<IBiasingSimulationState>();
                IBiasingSimulationState state;
                if (parameters.LocalBiasingSolver && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                    state = new LocalSimulationState(simulation.InstanceName, parent, new SparseRealSolver());
                else
                    state = new FlatSimulationState(simulation.InstanceName, parent, simulation.Nodes);
                simulation.LocalStates.Add(state);
            }
        }
        private readonly LocalSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public Biasing(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalStates.TryGetValue<LocalSimulationState>(out ISimulationState state))
            {
                _state = (LocalSimulationState)state;
                _state.Initialize(simulation.Nodes);
            }
            _convergenceBehaviors = simulation.EntityBehaviors.GetBehaviorList<IConvergenceBehavior>();
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
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

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            _state?.Update();
            var result = true;
            foreach (var behavior in _convergenceBehaviors)
                result &= behavior.IsConvergent();
            return result;
        }
    }
}
