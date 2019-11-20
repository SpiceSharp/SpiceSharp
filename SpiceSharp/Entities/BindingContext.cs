using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> to an <see cref="ISimulation"/>.
    /// </summary>
    public class BindingContext
    {
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        protected string Name { get; }

        /// <summary>
        /// Gets the simulation to bind to.
        /// </summary>
        /// <value>
        /// The simulation.
        /// </value>
        protected ISimulation Simulation { get; }

        /// <summary>
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        public virtual IBehaviorContainer Behaviors { get; }

        /// <summary>
        /// Gets the simulation variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public virtual IVariableSet Variables => Simulation.Variables;

        /// <summary>
        /// Gets the simulation configurations.
        /// </summary>
        /// <value>
        /// The configurations.
        /// </value>
        public virtual ParameterSetDictionary Configurations => Simulation.Configurations;

        /// <summary>
        /// Gets a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <returns>The simulation state.</returns>
        public S GetState<S>() where S : ISimulationState
        {
            if (Simulation is IStateful<S> sim)
                return sim.State;
            throw new CircuitException("The simulation does not use a state of type {0}".FormatString(typeof(S)));
        }

        /// <summary>
        /// Tries to get a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <param name="state">The simulation state.</param>
        /// <returns>
        /// <c>true</c> if the state was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetState<S>(out S state) where S : ISimulationState
        {
            if (Simulation is IStateful<S> sim)
            {
                state = sim.State;
                return true;
            }
            state = default;
            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The entity behaviors.</param>
        public BindingContext(ISimulation simulation, IBehaviorContainer behaviors)
        {
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            Behaviors = behaviors.ThrowIfNull(nameof(behaviors));
            Name = behaviors.Name;
        }
    }
}
