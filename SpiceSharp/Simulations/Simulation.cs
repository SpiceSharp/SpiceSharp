﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.General;
using SpiceSharp.Validation;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for any simulation.
    /// </summary>
    public abstract class Simulation : Parameterized, IEventfulSimulation,
        IParameterized<CollectionParameters>
    {
        /// <summary>
        /// Gets the current status of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public SimulationStatus Status { get; private set; }

        /// <summary>
        /// Gets the collection parameters.
        /// </summary>
        /// <value>
        /// The collection parameters.
        /// </value>
        public CollectionParameters CollectionParameters { get; } = new CollectionParameters();
        CollectionParameters IParameterized<CollectionParameters>.Parameters => CollectionParameters;

        /// <summary>
        /// Gets all the state types that are used by the class.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
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

        /// <summary>
        /// Gets all behavior types that are used by the class.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
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

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables { get; private set; }

        #region Events
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
        /// Occurs before the simulation starts validating the input.
        /// </summary>
        public event EventHandler<EventArgs> BeforeValidation;

        /// <summary>
        /// Occurs after the simulation has validated the input.
        /// </summary>
        public event EventHandler<EventArgs> AfterValidation;

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
        #endregion

        /// <summary>
        /// Gets the name of the simulation.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a pool of all entity behaviors active in the simulation.
        /// </summary>
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
        protected Simulation(string name)
        {
            Name = name;
            Statistics = new SimulationStatistics();
        }

        /// <summary>
        /// Runs the simulation on the specified circuit.
        /// </summary>
        /// <param name="entities">The entities to simulate.</param>
        public virtual void Run(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Setup the simulation
            OnBeforeSetup(EventArgs.Empty);
            Statistics.SetupTime.Start();
            Status = SimulationStatus.Setup;
            Setup(entities);
            Statistics.SetupTime.Stop();
            OnAfterSetup(EventArgs.Empty);

            // Validate the input
            OnBeforeValidation(EventArgs.Empty);
            Statistics.ValidationTime.Start();
            Status = SimulationStatus.Validation;
            Validate(entities);
            Statistics.ValidationTime.Stop();
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
                Execute();
                Statistics.ExecutionTime.Stop();

                // Reset
                afterArgs.Repeat = false;
                OnAfterExecute(afterArgs);

                // We're going to repeat the simulation, change the event arguments
                if (afterArgs.Repeat)
                    beforeArgs = new BeforeExecuteEventArgs(true);
            } while (afterArgs.Repeat);

            // Clean up the circuit
            OnBeforeUnsetup(EventArgs.Empty);
            Statistics.UnsetupTime.Start();
            Status = SimulationStatus.Unsetup;
            Unsetup();
            Statistics.UnsetupTime.Stop();
            OnAfterUnsetup(EventArgs.Empty);

            Status = SimulationStatus.None;
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The entities that are included in the simulation.</param>
        protected virtual void Setup(IEntityCollection entities)
        {
            // Validate the entities
            entities.ThrowIfNull(nameof(entities));
            if (entities.Count == 0)
                throw new SpiceSharpException(Properties.Resources.Simulations_NoEntities.FormatString(Name));

            // Create the set of variables
            Variables = CollectionParameters.Variables?.Invoke() ?? new VariableSet();
            Variables.Clear();

            // Create all entity behaviors
            CreateBehaviors(entities);
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="entities">The entities.</param>
        protected abstract void Validate(IEntityCollection entities);

        /// <summary>
        /// A default implementation for validating entities and behaviors using the specified rules.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <param name="entities">The entities.</param>
        /// <exception cref="SimulationValidationFailed">Thrown if the validation failed.</exception>
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
            foreach (var behavior in EntityBehaviors.SelectMany(p => p.Values))
            {
                if (behavior is IRuleSubject subject)
                    subject.Apply(rules);
            }

            // Are there still violated rules?
            if (rules.ViolationCount > 0)
                throw new SimulationValidationFailed(this, rules);
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected virtual void Unsetup()
        {
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// Creates all behaviors for the simulation.
        /// </summary>
        /// <param name="entities">The entities.</param>
        protected virtual void CreateBehaviors(IEntityCollection entities)
        {
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
            foreach (var entity in entities)
            {
                if (!EntityBehaviors.Contains(entity.Name))
                    entity.CreateBehaviors(this);
            }
            Statistics.BehaviorCreationTime.Stop();

            EntityBehaviors.BehaviorsNotFound -= BehaviorsNotFound;
        }

        /// <summary>
        /// Checks if the class uses the specified behaviors.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>
        /// <c>true</c> if the class uses the behavior; otherwise <c>false</c>.
        /// </returns>
        public virtual bool UsesBehaviors<B>() where B : IBehavior
        {
            if (this is IBehavioral<B>)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the state of the specified type.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// The type.
        /// </returns>
        public virtual S GetState<S>() where S : ISimulationState
        {
            if (this is IStateful<S> stateful)
                return stateful.State;
            return default;
        }

        /// <summary>
        /// Checks if the class uses the specified state.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the class uses the state; otherwise <c>false</c>.
        /// </returns>
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
