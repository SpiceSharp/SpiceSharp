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
    /// <seealso cref="ISimulation"/>
    public abstract class Simulation : ParameterSetCollection, ISimulation
    {
        private int _lastRun = -1;

        /// <summary>
        /// The bits that are reserved for exports.
        /// </summary>
        public const int Exports = 0x0000_ffff;

        /// <summary>
        /// The bits that are reserved for actions.
        /// </summary>
        public const int Actions = unchecked((int)0xffff_0000);

        /// <summary>
        /// Represents the action before setting up behaviors.
        /// </summary>
        public const int BeforeSetup = 0x0001_0000;

        /// <summary>
        /// Represents the action after setting up behaviors.
        /// </summary>
        public const int AfterSetup = 0x0002_0000;

        /// <summary>
        /// Represents the action before validation.
        /// </summary>
        public const int BeforeValidation = 0x0004_0000;

        /// <summary>
        /// Represents the action after validating.
        /// </summary>
        public const int AfterValidation = 0x0008_0000;
        
        /// <summary>
        /// Represents the action before each execution.
        /// </summary>
        public const int BeforeExecute = 0x0010_0000;
        
        /// <summary>
        /// Represents the action after each execution.
        /// </summary>
        public const int AfterExecute = 0x0020_0000;

        /// <summary>
        /// Represents the action before cleaning up.
        /// </summary>
        public const int BeforeUnsetup = 0x0040_0000;

        /// <summary>
        /// Represents the action after cleaning up.
        /// </summary>
        public const int AfterUnsetup = 0x0080_0000;

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

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc />
        public int CurrentRun { get; private set; } = -1;

        /// <inheritdoc />
        public bool Repeat { get; set; }

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
        public virtual IEnumerable<int> Run(IEntityCollection entities, int mask = Exports)
        {
            if (CurrentRun >= 0)
                throw new ArgumentException(Properties.Resources.Simulations_CannotRunMultiple);
            _lastRun++;
            CurrentRun = _lastRun;

            entities.ThrowIfNull(nameof(entities));

            // Yield before setup - this is here for easier migration from Spice# 3.1.x
            if ((mask & BeforeSetup) != 0)
                yield return BeforeSetup;

            // Setup the simulation
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

            // Yield after setup
            if ((mask & AfterSetup) != 0)
                yield return AfterSetup;

            // Yield before validation - this is here for easier migration from Spice# 3.1.x
            if ((mask & BeforeValidation) != 0)
                yield return BeforeValidation;

            // Validate the input
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

            // Yield after validation
            if ((mask & AfterValidation) != 0)
                yield return AfterValidation;

            // Execute the simulation
            Status = SimulationStatus.Running;
            do
            {
                Repeat = false;

                // Before execution
                if ((mask & BeforeExecute) != 0)
                    yield return BeforeExecute;

                // Execute simulation
                Statistics.ExecutionTime.Start();
                try
                {
                    foreach (int exportType in Execute(mask))
                        yield return exportType;
                }
                finally
                {
                    Statistics.ExecutionTime.Stop();
                }

                // Reset                
                if ((mask & AfterExecute) != 0)
                    yield return AfterExecute;

                // We're going to repeat the simulation, change the event arguments
            } while (Repeat);

            // Yield before cleanup
            if ((mask & BeforeUnsetup) != 0)
                yield return BeforeUnsetup;

            // Clean up the circuit
            Statistics.FinishTime.Start();
            try
            {
                Status = SimulationStatus.Unsetup;
                Finish();
            }
            finally
            {
                CurrentRun = -1;
                Statistics.FinishTime.Stop();
            }
            Status = SimulationStatus.None;

            // Yield after cleanup - this is here for easier migration from Spice# 3.1.x
            if ((mask & AfterUnsetup) != 0)
                yield return AfterUnsetup;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> Rerun(int mask = Exports)
        {
            if (CurrentRun >= 0)
                throw new ArgumentException(Properties.Resources.Simulations_CannotRunMultiple);
            _lastRun++;
            CurrentRun = _lastRun;

            // Execute the simulation
            Status = SimulationStatus.Running;
            do
            {
                Repeat = false;

                // Yield before execute
                if ((mask & BeforeExecute) != 0)
                    yield return BeforeExecute;

                // Execute simulation
                Statistics.ExecutionTime.Start();
                try
                {
                    foreach (int exportType in Execute(mask))
                        yield return exportType;
                }
                finally
                {
                    Statistics.ExecutionTime.Stop();
                }

                // Yield after execute
                if ((mask & AfterExecute) != 0)
                    yield return AfterExecute;
            } while (Repeat);

            // Yield before cleanup
            if ((mask & BeforeUnsetup) != 0)
                yield return BeforeUnsetup;

            // Clean up the circuit
            Statistics.FinishTime.Start();
            try
            {
                Status = SimulationStatus.Unsetup;
                Finish();
            }
            finally
            {
                CurrentRun = -1;
                Statistics.FinishTime.Stop();
            }
            Status = SimulationStatus.None;

            // Yield after cleanup - this is here for easier migration from Spice# 3.1.x
            if ((mask & AfterUnsetup) != 0)
                yield return AfterUnsetup;
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
        /// <param name="mask">A bit mask for simulation export identifiers.</param>
        /// <exception cref="SpiceSharpException">Thrown if the simulation can't continue.</exception>
        protected abstract IEnumerable<int> Execute(int mask);

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
    }
}
