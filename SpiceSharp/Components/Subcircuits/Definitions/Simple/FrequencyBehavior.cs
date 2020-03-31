using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyBehavior" />
    public partial class FrequencyBehavior : SubcircuitBehavior<IFrequencyBehavior>, IFrequencyBehavior
    {
        /// <summary>
        /// Prepares the specified simulation for frequency behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(SubcircuitSimulation simulation)
        {
            var parameters = simulation.GetParameterSet<BaseParameters>();
            if (simulation.UsesState<IComplexSimulationState>())
            {
                var parent = simulation.GetState<IComplexSimulationState>();
                IComplexSimulationState state;
                if (parameters.LocalComplexSolver && !simulation.LocalStates.ContainsKey(typeof(IComplexSimulationState)))
                    state = new LocalSimulationState(simulation.InstanceName, simulation.Nodes, parent, LUHelper.CreateSparseComplexSolver());
                else
                    state = new FlatSimulationState(simulation.InstanceName, simulation.Nodes, parent);
                simulation.LocalStates.Add(state);
            }
        }
        private readonly LocalSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public FrequencyBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalStates.TryGetValue(out _state))
                _state.Initialize(simulation.Nodes);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        public void InitializeParameters()
        {
            foreach (var behavior in Behaviors)
                behavior.InitializeParameters();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Load()
        {
            if (_state != null)
            {
                _state.Update();
                do
                {
                    _state.IsConvergent = true;
                    _state.Solver.Reset();
                    foreach (var behavior in Behaviors)
                        behavior.Load();
                }
                while (!_state.Apply());
            }
            else
            {
                foreach (var behavior in Behaviors)
                    behavior.Load();
            }
        }
    }
}
