using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A template for a subcircuit simulation.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public abstract class SubcircuitSimulation : IEventfulSimulation
    {
        /// <summary>
        /// Occurs when simulation data can be exported.
        /// </summary>
        public event EventHandler<ExportDataEventArgs> ExportSimulationData;

        /// <summary>
        /// Occurs before the simulation is set up.
        /// </summary>
        public event EventHandler<EventArgs> BeforeSetup;

        /// <summary>
        /// Occurs after the simulation is set up.
        /// </summary>
        public event EventHandler<EventArgs> AfterSetup;

        /// <summary>
        /// Occurs before the simulation starts its execution.
        /// </summary>
        public event EventHandler<BeforeExecuteEventArgs> BeforeExecute;

        /// <summary>
        /// Occurs after the simulation has executed.
        /// </summary>
        public event EventHandler<AfterExecuteEventArgs> AfterExecute;

        /// <summary>
        /// Occurs before the simulation is destroyed.
        /// </summary>
        public event EventHandler<EventArgs> BeforeUnsetup;

        /// <summary>
        /// Occurs after the simulation is destroyed.
        /// </summary>
        public event EventHandler<EventArgs> AfterUnsetup;

        /// <summary>
        /// Gets the parent simulation.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        protected ISimulation Parent { get; }

        /// <summary>
        /// Gets the name of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => Parent.Name;

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
        public ParameterSetDictionary Configurations => Parent.Configurations;

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables => Parent.Variables;

        /// <summary>
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The entity behaviors.
        /// </value>
        public IBehaviorContainerCollection EntityBehaviors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation"/> class.
        /// </summary>
        public SubcircuitSimulation(ISimulation parent)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Runs the <see cref="ISimulation" /> on the specified <see cref="IEntityCollection" />.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Run(IEntityCollection entities)
        {
            EntityBehaviors = new BehaviorContainerCollection(Parent.EntityBehaviors);

            void BehaviorsNotFound(object sender, BehaviorsNotFoundEventArgs args)
            {
                if (entities.TryGetEntity(args.Name, out var entity))
                {
                    var behaviors = new BehaviorContainer(entity.Name);
                    entity.CreateBehaviors(this, behaviors);
                    EntityBehaviors.Add(behaviors);
                    args.Behaviors = behaviors;
                }
                else
                {
                    // Try finding it in the parent simulation
                    args.Behaviors = Parent.EntityBehaviors[args.Name];
                }
            }
            EntityBehaviors.BehaviorsNotFound += BehaviorsNotFound;

            foreach (var entity in entities)
            {
                if (!EntityBehaviors.Contains(entity.Name))
                {
                    var behaviors = new BehaviorContainer(entity.Name);
                    entity.CreateBehaviors(this, behaviors);
                    EntityBehaviors.Add(behaviors);
                }
            }

            EntityBehaviors.BehaviorsNotFound -= BehaviorsNotFound;
        }
    }
}
