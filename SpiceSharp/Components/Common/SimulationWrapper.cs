using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.General;
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

        /// <inheritdoc />
        public int CurrentRun => Parent.CurrentRun;

        /// <inheritdoc />
        public bool Repeat
        {
            get => Parent.Repeat;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the local states. These states are returned first!
        /// </summary>
        /// <value>
        /// The local states.
        /// </value>
        public ITypeSet<ISimulationState> LocalStates { get; }

        /// <inheritdoc/>
        public IEnumerable<Type> States => Parent.States;

        /// <inheritdoc/>
        public IEnumerable<Type> Behaviors => Parent.Behaviors;

        /// <inheritdoc/>
        public string Name => Parent.Name;

        /// <inheritdoc/>
        public SimulationStatus Status => Parent.Status;

        /// <inheritdoc/>
        public IBehaviorContainerCollection EntityBehaviors { get; }

        /// <inheritdoc/>
        public virtual IEnumerable<IParameterSet> ParameterSets => Parent.ParameterSets;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationWrapper"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="states">The simulation states.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/>, <paramref name="behaviors"/> or <paramref name="states"/> is <c>null</c>.</exception>
        public SimulationWrapper(ISimulation parent,
            IBehaviorContainerCollection behaviors,
            ITypeSet<ISimulationState> states)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            EntityBehaviors = behaviors.ThrowIfNull(nameof(behaviors));
            LocalStates = states.ThrowIfNull(nameof(states));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The behaviors are stored in the specified <see cref="EntityBehaviors"/> of the <see cref="SimulationWrapper"/>.
        /// This can be a local collection, allowing you to keep a part of the behaviors separate.
        /// </remarks>
        public IEnumerable<int> Run(IEntityCollection entities, int exportMask = -1)
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

            yield break;
        }

        /// <inheritdoc/>
        public IEnumerable<int> Rerun(int exportMask = -1)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual S GetState<S>() where S : ISimulationState
        {
            if (LocalStates.TryGetValue(out S result))
                return result;
            return Parent.GetState<S>();
        }

        /// <inheritdoc/>
        public virtual bool TryGetState<S>(out S state) where S : ISimulationState
        {
            if (LocalStates.TryGetValue(out state))
                return true;
            return Parent.TryGetState(out state);
        }

        /// <summary>
        /// Gets the state of the parent simulation of the specified type, bypassing any local states.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// The state.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the simulation state is not defined.</exception>
        public S GetParentState<S>() where S : ISimulationState
            => Parent.GetState<S>();

        /// <summary>
        /// Tries the state of the get parent simulatio nof the specified type, bypassing any local states.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <param name="state">The simulation state.</param>
        /// <returns>
        ///   <c>true</c> if the simulation state exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetParentState<S>(out S state) where S : ISimulationState
            => Parent.TryGetState(out state);

        /// <inheritdoc/>
        public bool UsesState<S>() where S : ISimulationState => Parent.UsesState<S>();

        /// <inheritdoc/>
        public bool UsesBehaviors<B>() where B : IBehavior => Parent.UsesBehaviors<B>();

        /// <inheritdoc/>
        public bool UsesBehavior(Type behaviorType) => Parent.UsesBehavior(behaviorType);

        /// <inheritdoc/>
        public virtual P GetParameterSet<P>() where P : IParameterSet, ICloneable<P> => Parent.GetParameterSet<P>();

        /// <inheritdoc/>
        public virtual bool TryGetParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P>
            => Parent.TryGetParameterSet(out value);

        /// <inheritdoc/>
        public virtual void SetParameter<P>(string name, P value) => Parent.SetParameter(name, value);

        /// <inheritdoc/>
        public virtual bool TrySetParameter<P>(string name, P value) => Parent.TrySetParameter(name, value);

        /// <inheritdoc/>
        public virtual P GetProperty<P>(string name) => Parent.GetProperty<P>(name);

        /// <inheritdoc/>
        public virtual bool TryGetProperty<P>(string name, out P value) => Parent.TryGetProperty(name, out value);

        /// <inheritdoc/>
        public virtual Action<P> CreateParameterSetter<P>(string name) => Parent.CreateParameterSetter<P>(name);

        /// <inheritdoc/>
        public virtual Func<P> CreatePropertyGetter<P>(string name) => Parent.CreatePropertyGetter<P>(name);
    }
}
