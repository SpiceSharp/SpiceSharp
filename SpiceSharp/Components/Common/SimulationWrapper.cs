using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Common
{
    /// <summary>
    /// A common wrapper for a simulation. It allows an extra layer of an <see cref="ISimulation"/>.
    /// The wrapper can inject configurations, states, solved variables, etc.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public class SimulationWrapper : ISimulation
    {
        /// <summary>
        /// Gets the parent simulation.
        /// </summary>
        /// <value>
        /// The parent simulation.
        /// </value>
        protected ISimulation Parent { get; }

        /// <summary>
        /// Gets the local states.
        /// </summary>
        /// <value>
        /// The local states.
        /// </value>
        public ITypeDictionary<ISimulationState> LocalStates { get; }

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
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The entity behaviors.
        /// </value>
        public IBehaviorContainerCollection EntityBehaviors { get; }

        /// <summary>
        /// Gets all parameter sets.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        public virtual IEnumerable<IParameterSet> ParameterSets => Parent.ParameterSets;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationWrapper"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="states">The simulation states.</param>
        public SimulationWrapper(ISimulation parent, 
            IBehaviorContainerCollection behaviors,
            ITypeDictionary<ISimulationState> states)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            EntityBehaviors = behaviors.ThrowIfNull(nameof(behaviors));
            LocalStates = states.ThrowIfNull(nameof(states));
        }

        /// <summary>
        /// Runs the <see cref="ISimulation" /> on the specified <see cref="IEntityCollection" />.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Run(IEntityCollection entities)
        {
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
        /// The state, or <c>null</c> if the state isn't used.
        /// </returns>
        public virtual S GetState<S>() where S : ISimulationState
        {
            if (LocalStates.TryGetValue(out S result))
                return result;
            return default;
        }

        /// <summary>
        /// Gets the state of the parent simulation.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// The state, or <c>null</c> if the state isn't used.
        /// </returns>
        public S GetParentState<S>() where S : ISimulationState
            => Parent.GetState<S>();

        /// <summary>
        /// Checks if the class uses the specified state.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the class uses the state; otherwise <c>false</c>.
        /// </returns>
        public bool UsesState<S>() where S : ISimulationState => Parent.UsesState<S>();

        /// <summary>
        /// Checks if the class uses the specified behaviors.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the class uses the behavior; otherwise <c>false</c>.
        /// </returns>
        public bool UsesBehaviors<B>() where B : IBehavior => Parent.UsesBehaviors<B>();

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        public virtual P GetParameterSet<P>() where P : IParameterSet
            => Parent.GetParameterSet<P>();

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool TryGetParameterSet<P>(out P value) where P : IParameterSet
            => Parent.TryGetParameterSet(out value);
    }
}
