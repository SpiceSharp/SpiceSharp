using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> to an <see cref="SpiceSharp.Simulations.ISimulation"/>.
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
        public virtual BehaviorContainer Behaviors { get; }

        /// <summary>
        /// Gets the simulation variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public virtual IVariableSet Variables => Simulation.Variables;

        /// <summary>
        /// Gets the simulation states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public virtual TypeDictionary<SimulationState> States => Simulation.States;

        /// <summary>
        /// Gets the simulation configurations.
        /// </summary>
        /// <value>
        /// The configurations.
        /// </value>
        public virtual ParameterSetDictionary Configurations => Simulation.Configurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The entity behaviors.</param>
        public BindingContext(ISimulation simulation, BehaviorContainer behaviors)
        {
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            Behaviors = behaviors.ThrowIfNull(nameof(behaviors));
            Name = behaviors.Name;
        }
    }
}
