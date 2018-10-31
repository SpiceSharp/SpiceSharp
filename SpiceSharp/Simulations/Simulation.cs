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
        /// Gets the current status of the simulation.
        /// </summary>
        public Statuses Status { get; private set; } = Statuses.None;

        /// <summary>
        /// Gets a set of <see cref="ParameterSet" /> that hold the configurations for the simulation.
        /// </summary>
        /// <value>
        /// The dictionary with configurations.
        /// </value>
        public ParameterSetDictionary Configurations { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets the configuration parameter sets. Obsolete, use <see cref="Configurations" /> instead.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        [Obsolete] public ParameterSetDictionary ParameterSets => Configurations;

        /// <summary>
        /// Gets the set of variables (unknowns).
        /// </summary>
        /// <value>
        /// The set of variables.
        /// </value>
        public VariableSet Variables { get; private set; }

        /// <summary>
        /// Gets the nodes. Obsolete, use <see cref="Variables" /> instead.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        [Obsolete] public VariableSet Nodes => Variables;

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
        public BehaviorPool EntityBehaviors { get; private set; }

        /// <summary>
        /// Gets a pool of all entity parameter sets active in the simulation.
        /// </summary>
        public ParameterPool EntityParameters { get; private set; }

        // Private parameters
        private bool _cloneParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected Simulation(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Runs the simulation on the specified circuit.
        /// </summary>
        /// <param name="circuit">The circuit to simulate.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        /// <exception cref="CircuitException">{0}: No circuit nodes for simulation".FormatString(Name)</exception>
        public virtual void Run(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            
            // Setup the simulation
            OnBeforeSetup(EventArgs.Empty);
            Status = Statuses.Setup;
            Setup(circuit);
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
                Execute();

                // Reset
                afterArgs.Repeat = false;
                OnAfterExecute(afterArgs);

                // We're going to repeat the simulation, change the event arguments
                if (afterArgs.Repeat)
                    beforeArgs = new BeforeExecuteEventArgs(true);
            } while (afterArgs.Repeat);

            // Clean up the circuit
            OnBeforeUnsetup(EventArgs.Empty);
            Status = Statuses.Unsetup;
            Unsetup();
            OnAfterUnsetup(EventArgs.Empty);

            Status = Statuses.None;
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        /// <exception cref="CircuitException">{0}: No circuit objects for simulation".FormatString(Name)</exception>
        protected virtual void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // No use simulating an empty circuit
            if (circuit.Entities.Count == 0)
                throw new CircuitException("{0}: No circuit objects for simulation".FormatString(Name));

            // Use the same comparer as the circuit. This is crucial because they use the same identifiers!
            EntityParameters = new ParameterPool(circuit.Entities.Comparer);
            EntityBehaviors = new BehaviorPool(circuit.Entities.Comparer);

            // Create the variables that will need solving
            if (Configurations.TryGet(out CollectionConfiguration cconfig))
            {
                Variables = new VariableSet(cconfig.VariableComparer ?? EqualityComparer<string>.Default);
                _cloneParameters = cconfig.CloneParameters;
            }
            else
            {
                Variables = new VariableSet();
                _cloneParameters = false;
            }

            // Setup all objects
            circuit.Entities.BuildOrderedComponentList();

            // Get all parameters
            SetupParameters(circuit.Entities);
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
        /// Collect and set up the behaviors of all circuit entities.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <param name="entities">The entities for which behaviors need to be collected.</param>
        /// <returns>
        /// A list of behaviors.
        /// </returns>
        /// <exception cref="ArgumentNullException">entities</exception>
        protected BehaviorList<T> SetupBehaviors<T>(IEnumerable<Entity> entities) where T : IBehavior
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            EntityBehaviors.ListenTo(typeof(T));

            // Register all behaviors
            foreach (var entity in entities)
            {
                var behavior = entity.CreateBehavior<T>(this);
                if (behavior != null)
                    EntityBehaviors.Add(entity.Name, behavior);
            }
            return EntityBehaviors.GetBehaviorList<T>();
        }

        /// <summary>
        /// Collect and set up the parameter sets of all circuit entities.
        /// </summary>
        /// <remarks>
        /// The parameter sets are cloned during set up to avoid issues when running multiple
        /// simulations in parallel.
        /// </remarks>
        /// <param name="entities">The entities for which parameter sets need to be collected.</param>
        /// <exception cref="ArgumentNullException">entities</exception>
        private void SetupParameters(IEnumerable<Entity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            // Register all parameters
            foreach (var entity in entities)
            {
                foreach (var p in entity.ParameterSets.Values)
                {
                    p.CalculateDefaults();
                    EntityParameters.Add(entity.Name, _cloneParameters ? p.DeepClone() : p);
                }
            }
        }
    }
}
