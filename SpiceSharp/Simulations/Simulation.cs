using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for any simulation.
    /// </summary>
    /// <seealso cref="ParameterSetCollection"/>
    /// <seealso cref="IEventfulSimulation"/>
    public abstract class Simulation : ParameterSetCollection,
        IEventfulSimulation
    {
        /// <inheritdoc/>
        public SimulationStatus Status { get; private set; }

        /// <inheritdoc/>
        public virtual IEnumerable<Type> States
        {
            get
            {
                foreach (var i in InterfaceCache.Get(GetType()))
                {
                    var info = i.GetTypeInfo();
                    if (info.IsGenericType && info.GetGenericTypeDefinition() == typeof(IStateful<>))
                        yield return info.GetGenericArguments()[0];
                }
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Type> Behaviors
        {
            get
            {
                var ifs = GetType().GetTypeInfo().GetInterfaces();
                foreach (var i in ifs)
                {
                    var info = i.GetTypeInfo();
                    if (info.IsGenericType && info.GetGenericTypeDefinition() == typeof(IBehavioral<>))
                        yield return info.GetGenericArguments()[0];
                }
            }
        }

        #region Events
        /// <inheritdoc/>
        public event EventHandler<ExportDataEventArgs> ExportSimulationData;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> BeforeSetup;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> AfterSetup;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> BeforeValidation;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> AfterValidation;

        /// <inheritdoc/>
        public event EventHandler<BeforeExecuteEventArgs> BeforeExecute;

        /// <inheritdoc/>
        public event EventHandler<AfterExecuteEventArgs> AfterExecute;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> BeforeUnsetup;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> AfterUnsetup;
        #endregion

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IBehaviorContainerCollection EntityBehaviors { get; private set; }

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public SimulationStatistics Statistics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected Simulation(string name)
        {
            Name = name.ThrowIfNull(nameof(name));
            Statistics = new SimulationStatistics();
        }

        /// <inheritdoc/>
        public virtual void Run(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Setup the simulation
            OnBeforeSetup(EventArgs.Empty);
            Statistics.SetupTime.Start();
            try
            {
                Status = SimulationStatus.Setup;
                Setup(entities);
            }
            finally
            {
                Statistics.SetupTime.Stop();
            }
            OnAfterSetup(EventArgs.Empty);

            // Validate the input
            OnBeforeValidation(EventArgs.Empty);
            Statistics.ValidationTime.Start();
            try
            {
                Status = SimulationStatus.Validation;
                Validate(entities);
            }
            finally
            {
                Statistics.ValidationTime.Stop();
            }
            OnAfterValidation(EventArgs.Empty);

            // Execute the simulation
            Status = SimulationStatus.Running;
            var beforeArgs = new BeforeExecuteEventArgs(false);
            var afterArgs = new AfterExecuteEventArgs();
            do
            {
                // Before execution
                OnBeforeExecute(beforeArgs);

                // Execute simulation
                Statistics.ExecutionTime.Start();
                try
                {
                    Execute();
                }
                finally
                {
                    Statistics.ExecutionTime.Stop();
                }

                // Reset
                afterArgs.Repeat = false;
                OnAfterExecute(afterArgs);

                // We're going to repeat the simulation, change the event arguments
                if (afterArgs.Repeat)
                    beforeArgs = new BeforeExecuteEventArgs(true);
            } while (afterArgs.Repeat);

            // Clean up the circuit
            OnBeforeUnsetup(EventArgs.Empty);
            Statistics.FinishTime.Start();
            try
            {
                Status = SimulationStatus.Unsetup;
                Finish();
            }
            finally
            {
                Statistics.FinishTime.Stop();
            }
            OnAfterUnsetup(EventArgs.Empty);

            Status = SimulationStatus.None;
        }

        /// <inheritdoc/>
        public virtual void Rerun()
        {
            // Execute the simulation
            Status = SimulationStatus.Running;
            var beforeArgs = new BeforeExecuteEventArgs(false);
            var afterArgs = new AfterExecuteEventArgs();
            do
            {
                // Before execution
                OnBeforeExecute(beforeArgs);

                // Execute simulation
                Statistics.ExecutionTime.Start();
                try
                {
                    Execute();
                }
                finally
                {
                    Statistics.ExecutionTime.Stop();
                }

                // Reset
                afterArgs.Repeat = false;
                OnAfterExecute(afterArgs);

                // We're going to repeat the simulation, change the event arguments
                if (afterArgs.Repeat)
                    beforeArgs = new BeforeExecuteEventArgs(true);
            } while (afterArgs.Repeat);

            // Clean up the circuit
            OnBeforeUnsetup(EventArgs.Empty);
            Statistics.FinishTime.Start();
            try
            {
                Status = SimulationStatus.Unsetup;
                Finish();
            }
            finally
            {
                Statistics.FinishTime.Stop();
            }
            OnAfterUnsetup(EventArgs.Empty);

            Status = SimulationStatus.None;
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The entities that are included in the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
        private void Setup(IEntityCollection entities)
        {
            // Validate the entities
            entities.ThrowIfNull(nameof(entities));
            if (entities.Count == 0)
            {
                // No entities! Don't stop here, but at least warn the user.
                SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_NoEntities.FormatString(Name));
            }

            // Create all simulation states
            CreateStates();

            // Create all entity behaviors (using the created simulation states)
            CreateBehaviors(entities);
        }

        /// <summary>
        /// Creates all the simulation states for the simulation.
        /// </summary>
        protected abstract void CreateStates();

        /// <summary>
        /// Creates all behaviors for the simulation.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
        protected virtual void CreateBehaviors(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));
            EntityBehaviors = new BehaviorContainerCollection(entities.Comparer);

            // Automatically create the behaviors of entities that need priority
            void BehaviorsNotFound(object sender, BehaviorsNotFoundEventArgs args)
            {
                if (entities.TryGetEntity(args.Name, out var entity))
                {
                    entity.CreateBehaviors(this);
                    if (EntityBehaviors.TryGetBehaviors(entity.Name, out var container))
                        args.Behaviors = container;
                }
            }
            EntityBehaviors.BehaviorsNotFound += BehaviorsNotFound;

            // Create the behaviors
            Statistics.BehaviorCreationTime.Start();
            try
            {
                foreach (var entity in entities)
                {
                    if (!EntityBehaviors.Contains(entity.Name))
                        entity.CreateBehaviors(this);
                }
            }
            finally
            {
                Statistics.BehaviorCreationTime.Stop();
            }

            EntityBehaviors.BehaviorsNotFound -= BehaviorsNotFound;
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
        /// <exception cref="ValidationFailedException">Thrown if the validation failed.</exception>
        protected abstract void Validate(IEntityCollection entities);

        /// <summary>
        /// A default implementation for validating entities and behaviors using the specified rules.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <param name="entities">The entities.</param>
        /// <exception cref="ValidationFailedException">Thrown if the validation failed.</exception>
        protected void Validate(IRules rules, IEntityCollection entities)
        {
            if (rules == null)
                return;
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity is IRuleSubject subject)
                        subject.Apply(rules);
                }
            }
            foreach (var behavior in EntityBehaviors.SelectMany(p => p))
            {
                if (behavior is IRuleSubject subject)
                    subject.Apply(rules);
            }

            // Are there still violated rules?
            if (rules.ViolationCount > 0)
                throw new ValidationFailedException(this, rules);
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown if the simulation can't continue.</exception>
        protected abstract void Execute();

        /// <summary>
        /// Finish the simulation.
        /// </summary>
        protected abstract void Finish();

        /// <inheritdoc/>
        public virtual bool UsesBehaviors<B>() where B : IBehavior
        {
            if (this is IBehavioral<B>)
                return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool UsesBehavior(Type behaviorType) => Behaviors.Any(b => b.Equals(behaviorType));

        /// <inheritdoc/>
        public virtual S GetState<S>() where S : ISimulationState
        {
            if (this is IStateful<S> stateful)
                return stateful.State;
            throw new TypeNotFoundException(typeof(S), Properties.Resources.Stateful_NotDefined.FormatString(typeof(S).FullName));
        }

        /// <inheritdoc/>
        public virtual bool TryGetState<S>(out S state) where S : ISimulationState
        {
            if (this is IStateful<S> stateful)
            {
                state = stateful.State;
                return true;
            }
            state = default;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool UsesState<S>() where S : ISimulationState
            => this is IStateful<S>;

        #region Methods for raising events
        /// <summary>
        /// Raises the <see cref="ExportSimulationData" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ExportDataEventArgs"/> instance containing the event data.</param>
        protected virtual void OnExport(ExportDataEventArgs args) => ExportSimulationData?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="BeforeSetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeSetup(EventArgs args) => BeforeSetup?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="BeforeExecute" /> event.
        /// </summary>
        /// <param name="args">The <see cref="BeforeExecuteEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeExecute(BeforeExecuteEventArgs args) => BeforeExecute?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterSetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterSetup(EventArgs args) => AfterSetup?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="BeforeValidation" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeValidation(EventArgs args) => BeforeValidation?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterValidation" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterValidation(EventArgs args) => AfterValidation?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterSetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterExecute(AfterExecuteEventArgs args) => AfterExecute?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="BeforeUnsetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeUnsetup(EventArgs args) => BeforeUnsetup?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterUnsetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterUnsetup(EventArgs args) => AfterUnsetup?.Invoke(this, args);
        #endregion
    }
}
