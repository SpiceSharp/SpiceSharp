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
            if (simulation.TryGetParameterSet(out FrequencyParameters result))
            {
                if (result.LocalSolver && !simulation.LocalStates.ContainsKey(typeof(IComplexSimulationState)))
                {
                    var parent = simulation.GetState<IComplexSimulationState>();
                    var state = new SimulationState(parent, LUHelper.CreateSparseComplexSolver());
                    simulation.LocalStates.Add(state);
                }
            }
        }
        private readonly SimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public FrequencyBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalStates.TryGetValue(out _state))
                _state.Initialize(simulation.SharedVariables);
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
