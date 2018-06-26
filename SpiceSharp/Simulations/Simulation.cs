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

        /// <summary>
        /// Event that is called for initializing simulation data exports
        /// </summary>
        public event EventHandler<EventArgs> InitializeSimulationExport;

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event EventHandler<ExportDataEventArgs> OnExportSimulationData;

        /// <summary>
        /// Event that is called for finalizing simulation data exports
        /// </summary>
        public event EventHandler<EventArgs> FinalizeSimulationExport;

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
        /// Run the simulation
        /// </summary>
        /// <param name="circuit">Circuit</param>
        public void Run(Circuit circuit) => Run(circuit, null);

        /// <summary>
        /// Run the simulation using a circuit
        /// </summary>
        /// <param name="circuit">Circuit</param>
        /// <param name="controller">Simulation flow controller</param>
        public virtual void Run(Circuit circuit, SimulationFlowController controller)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Setup the simulation
            Setup(circuit);
            InitializeSimulationExport?.Invoke(this, EventArgs.Empty);

            // Check that at least something is simulated
            if (Nodes.Count < 1)
                throw new CircuitException("{0}: No circuit nodes for simulation".FormatString(Name));

            // Execute the simulation
            if (controller != null)
            {
                controller.Initialize(this);
                do
                {
                    Execute();
                } while (controller.ContinueExecution(this));

                controller.Finalize(this);
            }
            else
                Execute();

            // Finalize the simulation
            FinalizeSimulationExport?.Invoke(this, EventArgs.Empty);
            Unsetup();
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

        /// <summary>
        /// Export the data
        /// </summary>
        protected void Export(ExportDataEventArgs args)
        {
            OnExportSimulationData?.Invoke(this, args);
        }

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
            foreach (Entity entity in entities)
            {
                T behavior = entity.CreateBehavior<T>(this);
                if (behavior != null)
                    EntityBehaviors.Add(entity.Name, behavior);
            }
            return EntityBehaviors.GetBehaviorList<T>();
        }

        /// <summary>
        /// Collect parameter sets of all circuit entities
        /// </summary>
        protected void SetupParameters(IEnumerable<Entity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            // Register all parameters
            foreach (Entity entity in entities)
            {
                foreach (ParameterSet p in entity.ParameterSets.Values)
                    EntityParameters.Add(entity.Name, p.DeepClone());
            }
        }
    }
}
