using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for any simulation.
    /// </summary>
    public abstract class Simulation
    {
        /// <summary>
        /// Possible statuses for a simulation.
        /// </summary>
        public enum Statuses
        {
            /// <summary>
            /// Indicates that the simulation has not started.
            /// </summary>
            None,

            /// <summary>
            /// Indicates that the simulation is now in its setup phase.
            /// </summary>
            Setup,

            /// <summary>
            /// Indicates that the simulation is running.
            /// </summary>
            Running,

            /// <summary>
            /// Indicates that the simulation is cleaning up all its resources.
            /// </summary>
            Unsetup
        }

        /// <summary>
        /// Gets or sets the current status of the simulation.
        /// </summary>
        public Statuses Status { get; protected set; } = Statuses.None;

        /// <summary>
        /// Gets a set of <see cref="ParameterSet" /> that hold the configurations for the simulation.
        /// </summary>
        public ParameterSetDictionary Configurations { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets a set of <see cref="SimulationState"/> objects used by the simulation.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public TypeDictionary<SimulationState> States { get; } = new TypeDictionary<SimulationState>();

        /// <summary>
        /// Gets a set of <see cref="ParameterSet" /> that holds the statistics for the simulation.
        /// </summary>
        public TypeDictionary<Statistics> Statistics { get; } = new TypeDictionary<Statistics>();

        /// <summary>
        /// Gets the set of variables (unknowns).
        /// </summary>
        /// <value>
        /// The set of variables.
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
        public BehaviorPool EntityBehaviors { get; protected set; }

        /// <summary>
        /// Gets a pool of all entity parameter sets active in the simulation.
        /// </summary>
        public ParameterPool EntityParameters { get; protected set; }

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
        protected List<Type> BehaviorTypes { get; } = new List<Type>(6);

        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected Simulation(string name)
        {
            Name = name;
            Statistics.Add(typeof(SimulationStatistics), SimulationStatistics);

            // Initialize
            States = new TypeDictionary<SimulationState>();
        }

        /// <summary>
        /// Runs the simulation on the specified circuit.
        /// </summary>
        /// <param name="entities">The entities to simulate.</param>
        public virtual void Run(EntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));
            
            // Setup the simulation
            OnBeforeSetup(EventArgs.Empty);
            SimulationStatistics.SetupTime.Start();
            Status = Statuses.Setup;
            Setup(entities);
            SimulationStatistics.SetupTime.Stop();
            OnAfterSetup(EventArgs.Empty);

            // Check that at least something is simulated
            if (Variables.Count < 1)
                throw new CircuitException("{0}: No circuit nodes for simulation".FormatString(Name));

            // Execute the simulation
            Status = Statuses.Running;
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
            Status = Statuses.Unsetup;
            Unsetup();
            SimulationStatistics.UnsetupTime.Stop();
            OnAfterUnsetup(EventArgs.Empty);

            Status = Statuses.None;
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The entities that are included in the simulation.</param>
        protected virtual void Setup(EntityCollection entities)
        {
            // Validate the entities
            entities.ThrowIfNull(nameof(entities));
            if (entities.Count == 0)
                throw new CircuitException("{0}: No circuit objects for simulation".FormatString(Name));

            // Create the variable set
            Variables = CreateVariableSet(entities);

            // Setup all entity parameters and behaviors
            CopyParameters(entities);
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
            EntityParameters.Clear();
            EntityParameters = null;

            // Clear all nodes
            Variables.Clear();
            Variables = null;
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected abstract void Execute();

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

        /// <summary>
        /// Creates the variable set.
        /// </summary>
        /// <param name="entities">The entities.</param>
        protected virtual IVariableSet CreateVariableSet(EntityCollection entities)
        {
            // Check the configuration for one, else just copy our own one
            if (Configurations.TryGet(out CollectionConfiguration cconfig))
                return cconfig.Variables ?? new VariableSet();
            else
                return new VariableSet();
        }

        /// <summary>
        /// Set up all behaviors previously created.
        /// </summary>
        /// <param name="entities">The circuit entities.</param>
        protected virtual void CreateBehaviors(EntityCollection entities)
        {
            // Create the behavior pool
            EntityBehaviors = new BehaviorPool(entities.Comparer, BehaviorTypes.ToArray());

            // Keep track of how long we are taking to create behaviors
            SimulationStatistics.BehaviorCreationTime.Start();

            // Create the behaviors
            var types = BehaviorTypes.ToArray();
            foreach (var entity in entities)
                entity.CreateBehaviors(types, this, entities);

            SimulationStatistics.BehaviorCreationTime.Stop();
        }

        /// <summary>
        /// Copy all parameter sets of the entities to the parameter pool.
        /// </summary>
        /// <remarks>
        /// The parameter sets are cloned during set up to avoid issues when running multiple
        /// simulations in parallel.
        /// </remarks>
        /// <param name="entities">The entities for which parameter sets need to be collected.</param>
        protected virtual void CopyParameters(EntityCollection entities)
        {
            // Create the parameter pool
            EntityParameters = new ParameterPool(entities.Comparer);

            // Check if we need to clone parameters
            bool _clone = false;
            if (Configurations.TryGet<CollectionConfiguration>(out var config))
                _clone = config.CloneParameters;

            // Register all parameters
            foreach (var entity in entities)
            {
                foreach (var p in entity.ParameterSets.Values)
                {
                    var parameterset = _clone ? p.Clone() : p;
                    parameterset.CalculateDefaults();
                    EntityParameters.Add(entity.Name, parameterset);
                }
            }
        }
    }
}
