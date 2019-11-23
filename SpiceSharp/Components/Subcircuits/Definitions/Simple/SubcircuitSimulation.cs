using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// A template for a subcircuit simulation.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public class SubcircuitSimulation : ISimulation
    {
        /// <summary>
        /// Gets the parent simulation.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        protected ISimulation Parent { get; }

        /// <summary>
        /// Gets all the states that the class uses.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public IEnumerable<Type> States => Parent.States;

        /// <summary>
        /// Gets all behavior types that are used by the class.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        public IEnumerable<Type> Behaviors => Parent.Behaviors;

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
        public IVariableSet Variables { get; }

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
        public SubcircuitSimulation(string name, ISimulation parent)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            Variables = new SubcircuitVariableSet(name, parent.Variables);
        }

        /// <summary>
        /// Runs the <see cref="ISimulation" /> on the specified <see cref="IEntityCollection" />.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Run(IEntityCollection entities)
        {
            EntityBehaviors = new BehaviorContainerCollection(Parent.EntityBehaviors.Comparer);

            void BehaviorsNotFound(object sender, BehaviorsNotFoundEventArgs args)
            {
                if (entities.TryGetEntity(args.Name, out var entity))
                {
                    entity.CreateBehaviors(this);
                    if (EntityBehaviors.TryGetBehaviors(entity.Name, out var container))
                        args.Behaviors = container;
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
                    entity.CreateBehaviors(this);
            }

            EntityBehaviors.BehaviorsNotFound -= BehaviorsNotFound;
        }

        /// <summary>
        /// Gets the state of the specified type.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// The type, or <c>null</c> if the state isn't used.
        /// </returns>
        public S GetState<S>() where S : ISimulationState => Parent.GetState<S>();

        /// <summary>
        /// Checks if the class uses the specified behaviors.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the class uses the behavior; otherwise <c>false</c>.
        /// </returns>
        public bool UsesBehaviors<B>() where B : IBehavior => Parent.UsesBehaviors<B>();
    }
}
