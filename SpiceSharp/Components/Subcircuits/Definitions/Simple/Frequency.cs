using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyBehavior" />
    public partial class Frequency : SubcircuitBehavior<IFrequencyBehavior>,
        IFrequencyBehavior
    {
        /// <summary>
        /// Prepares the specified simulation for frequency behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public static void Prepare(SubcircuitSimulation simulation)
        {
            var parameters = simulation.GetParameterSet<Parameters>();
            if (simulation.UsesState<IComplexSimulationState>())
            {
                var parent = simulation.GetState<IComplexSimulationState>();
                IComplexSimulationState state;
                if (parameters.LocalComplexSolver && !simulation.LocalStates.ContainsKey(typeof(IComplexSimulationState)))
                    state = new LocalSimulationState(simulation.InstanceName, parent, new SparseComplexSolver());
                else
                    state = new FlatSimulationState(simulation.InstanceName, parent, simulation.Nodes);
                simulation.LocalStates.Add(state);
            }
        }
        private readonly LocalSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public Frequency(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalStates.TryGetValue(out _state))
                _state.Initialize(simulation.Nodes);
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            foreach (var behavior in Behaviors)
                behavior.InitializeParameters();
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
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
