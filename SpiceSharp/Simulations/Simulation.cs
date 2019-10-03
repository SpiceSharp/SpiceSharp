using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for any simulation.
    /// </summary>
    public abstract class Simulation : ISimulation
    {
        /// <summary>
        /// Gets the current status of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public SimulationStatus Status { get; private set; }

        /// <summary>
        /// Gets a set of configurations for the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ParameterSetDictionary Configurations { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets a set of <see cref="SimulationState" /> instances used by the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public TypeDictionary<SimulationState> States { get; } = new TypeDictionary<SimulationState>();

        /// <summary>
        /// Gets a set of <see cref="Statistics" /> instances tracked by the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public TypeDictionary<Statistics> Statistics { get; } = new TypeDictionary<Statistics>();

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
        /// Gets the identifier of the simulation.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a pool of all entity behaviors active in the simulation.
        /// </summary>
        public BehaviorContainerCollection EntityBehaviors { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IBehavior" /> types used by the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The behavior types.
        /// </value>
        public IEnumerable<Type> BehaviorTypes => Types;

        /// <summary>
        /// A reference to the regular simulation statistics (cached)
        /// </summary>
        protected SimulationStatistics SimulationStatistics { get; } = new SimulationStatistics();

        /// <summary>
        /// Gets the behavior types in the order that they are called.
        /// </summary>
        /// <remarks>
        /// The order is important for establishing dependencies. A behavior that is called first should
        /// not depend on any other behaviors!
        /// </remarks>
        protected List<Type> Types { get; } = new List<Type>(6);

        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected Simulation(string name)
        {
            Name = name;

            // Add our own statistics
            Statistics.Add(SimulationStatistics);
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
            SimulationStatistics.SetupTime.Start();
            Status = SimulationStatus.Setup;
            Setup(entities);
            SimulationStatistics.SetupTime.Stop();
            OnAfterSetup(EventArgs.Empty);

            // Check that at least something is simulated
            if (Variables.Count < 1)
                throw new CircuitException("{0}: No circuit nodes for simulation".FormatString(Name));

            // Execute the simulation
            Status = SimulationStatus.Running;
            var beforeArgs = new BeforeExecuteEventArgs(false);
            var afterArgs = new AfterExecuteEventArgs();
            do
            {
                // Before execution
                OnBeforeExecute(beforeArgs);

                // Execute simulation
                SimulationStatistics.ExecutionTime.Start();
                Execute();
                SimulationStatistics.ExecutionTime.Stop();

                // Reset
                afterArgs.Repeat = false;
                OnAfterExecute(afterArgs);

                // We're going to repeat the simulation, change the event arguments
                if (afterArgs.Repeat)
                    beforeArgs = new BeforeExecuteEventArgs(true);
            } while (afterArgs.Repeat);

            // Clean up the circuit
            OnBeforeUnsetup(EventArgs.Empty);
            SimulationStatistics.UnsetupTime.Start();
            Status = SimulationStatus.Unsetup;
            Unsetup();
            SimulationStatistics.UnsetupTime.Stop();
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
                throw new CircuitException("{0}: No circuit objects for simulation".FormatString(Name));

            // Create the set of variables
            if (Configurations.TryGetValue(out CollectionConfiguration cconfig))
                Variables = cconfig.Variables ?? new VariableSet();
            else
                Variables = new VariableSet();

            // Create all entity behaviors
            CreateBehaviors(entities);
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected virtual void Unsetup()
        {
            // Clear all parameters
            EntityBehaviors.Clear();
            EntityBehaviors = null;

            // Clear all nodes
            Variables.Clear();
            Variables = null;
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// Create all behaviors for the simulation.
        /// </summary>
        /// <param name="entities">The entities.</param>
        protected virtual void CreateBehaviors(IEntityCollection entities)
        {
            EntityBehaviors = new BehaviorContainerCollection(entities.Comparer, Types.ToArray());

            // Create the behaviors
            SimulationStatistics.BehaviorCreationTime.Start();
            foreach (var entity in entities)
                entity.CreateBehaviors(this, entities);
            SimulationStatistics.BehaviorCreationTime.Stop();
        }

        #region Methods for raising events
        /// <summary>
        /// Raises the <see cref="E:ExportSimulationData" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ExportDataEventArgs"/> instance containing the event data.</param>
        protected virtual void OnExport(ExportDataEventArgs args) => ExportSimulationData?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:BeforeSetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeSetup(EventArgs args) => BeforeSetup?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:BeforeExecute" /> event.
        /// </summary>
        /// <param name="args">The <see cref="BeforeExecuteEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeExecute(BeforeExecuteEventArgs args) => BeforeExecute?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:AfterSetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterSetup(EventArgs args) => AfterSetup?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:AfterSetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterExecute(AfterExecuteEventArgs args) => AfterExecute?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:BeforeUnsetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeUnsetup(EventArgs args) => BeforeUnsetup?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:AfterUnsetup" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterUnsetup(EventArgs args) => AfterUnsetup?.Invoke(this, args);
        #endregion
    }
}
