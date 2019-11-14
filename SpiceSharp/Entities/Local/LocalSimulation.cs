using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.Local
{
    /// <summary>
    /// A simulation that can be used to capture behaviors of a subset of entities.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public abstract class LocalSimulation : ISimulation
    {
        /// <summary>
        /// Gets the parent simulation.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ISimulation Parent { get; }

        /// <summary>
        /// Gets the name of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get => _name ?? Parent.Name;
            protected set => _name = value;
        }
        private string _name = null;

        /// <summary>
        /// Gets the current status of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public SimulationStatus Status => Parent.Status;

        /// <summary>
        /// Gets a set of configurations for the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ParameterSetDictionary Configurations
        {
            get => _configurations ?? Parent.Configurations;
            protected set => _configurations = value;
        }
        private ParameterSetDictionary _configurations = null;

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables
        {
            get => _variables ?? Parent.Variables;
            protected set => _variables = value;
        }
        private IVariableSet _variables = null;

        /// <summary>
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The entity behaviors.
        /// </value>
        public BehaviorContainerCollection EntityBehaviors
        {
            get => _entityBehaviors ?? Parent.EntityBehaviors;
            protected set => _entityBehaviors = value;
        }
        private BehaviorContainerCollection _entityBehaviors = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSimulation"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation.</param>
        public LocalSimulation(ISimulation parent)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Runs the <see cref="ISimulation" /> on the specified <see cref="IEntityCollection" />.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void Run(IEntityCollection entities)
        {
            foreach (var entity in entities)
                entity.CreateBehaviors(this, entities);
        }
    }
}
