using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can perform a simulation
    /// </summary>
    public abstract class Simulation
    {
        /// <summary>
        /// Simulation configuration
        /// </summary>
        public ParameterSetDictionary ParameterSets { get; } = new ParameterSetDictionary();

        /// <summary>
        /// States of the simulation
        /// </summary>
        public StateDictionary States { get; } = new StateDictionary();

        /// <summary>
        /// Gets the node map for this simulation
        /// </summary>
        public VariableSet Nodes { get; } = new VariableSet();

        #region Events
        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event EventHandler<ExportDataEventArgs> ExportSimulationData;

        /// <summary>
        /// Event called before setting up the circuit
        /// </summary>
        public event EventHandler<EventArgs> BeforeSetup;

        /// <summary>
        /// Event called after setting up the circuit
        /// </summary>
        public event EventHandler<EventArgs> AfterSetup;

        /// <summary>
        /// Event called after executing the simulation
        /// </summary>
        public event EventHandler<SimulationFlowEventArgs> AfterExecute; 

        /// <summary>
        /// Event called before cleaning up the circuit
        /// </summary>
        public event EventHandler<EventArgs> BeforeUnsetup;

        /// <summary>
        /// Event called after cleaning up the circuit
        /// </summary>
        public event EventHandler<EventArgs> AfterUnsetup; 
        #endregion

        /// <summary>
        /// Gets the name of the simulation
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Gets a pool of all behaviors active in the simulation
        /// </summary>
        public BehaviorPool EntityBehaviors { get; } = new BehaviorPool();

        /// <summary>
        /// Gets a pool of all parameters active in the simulation
        /// </summary>
        public ParameterPool EntityParameters { get; } = new ParameterPool();

        /// <summary>
        /// Constructor
        /// </summary>
        protected Simulation(Identifier name)
        {
            Name = name;
        }

        /// <summary>
        /// Run the simulation using a circuit
        /// </summary>
        /// <param name="circuit">Circuit</param>
        public virtual void Run(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Setup the simulation
            OnBeforeSetup(EventArgs.Empty);
            Setup(circuit);
            OnAfterSetup(EventArgs.Empty);

            // Check that at least something is simulated
            if (Nodes.Count < 1)
                throw new CircuitException("{0}: No circuit nodes for simulation".FormatString(Name));

            // Execute the simulation
            var args = new SimulationFlowEventArgs();
            while (args.Repeat)
            {
                Execute();

                args.Repeat = false;
                AfterExecute?.Invoke(this, args);
            }

            // Clean up the circuit
            OnBeforeUnsetup(EventArgs.Empty);
            Unsetup();
            OnAfterUnsetup(EventArgs.Empty);
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        /// <param name="circuit">Circuit</param>
        protected virtual void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // No use simulating an empty circuit
            if (circuit.Objects.Count == 0)
                throw new CircuitException("{0}: No circuit objects for simulation".FormatString(Name));

            // Setup all objects
            circuit.Objects.BuildOrderedComponentList();

            // Get all parameters
            SetupParameters(circuit.Objects);
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected virtual void Unsetup()
        {
            // Clear all parameters
            EntityBehaviors.Clear();
            EntityParameters.Clear();

            // Clear all nodes
            Nodes.Clear();

            // Clear all states
            States.Clear();
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        protected abstract void Execute();

        #region Methods for raising events
        /// <summary>
        /// Call event for exporting data
        /// </summary>
        protected virtual void OnExport(ExportDataEventArgs args) => ExportSimulationData?.Invoke(this, args);

        /// <summary>
        /// Call event to indicate we're about to set up the simulation
        /// </summary>
        protected virtual void OnBeforeSetup(EventArgs args) => BeforeSetup?.Invoke(this, args);

        /// <summary>
        /// Call event just after setting up the simulation
        /// </summary>
        protected virtual void OnAfterSetup(EventArgs args) => AfterSetup?.Invoke(this, args);

        /// <summary>
        /// Call event just before cleaning up the circuit
        /// </summary>
        protected virtual void OnBeforeUnsetup(EventArgs args) => BeforeUnsetup?.Invoke(this, args);

        /// <summary>
        /// Call event just after cleaning up the circuit
        /// </summary>
        protected virtual void OnAfterUnsetup(EventArgs args) => AfterUnsetup?.Invoke(this, args);
        #endregion

        /// <summary>
        /// Collect behaviors of all circuit entities while also setting them up
        /// </summary>
        /// <typeparam name="T">Base behavior</typeparam>
        /// <returns></returns>
        protected BehaviorList<T> SetupBehaviors<T>(IEnumerable<Entity> entities) where T : Behavior
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

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
        /// Collect parameter sets of all circuit entities
        /// </summary>
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
                    EntityParameters.Add(entity.Name, p.DeepClone());
                }
            }
        }
    }
}
