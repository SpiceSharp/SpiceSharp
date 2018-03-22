using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can perform a simulation using a <see cref="SpiceSharp.Circuit"/>.
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
        public NodeMap Nodes { get; } = new NodeMap();

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
        /// <param name="circuit"></param>
        protected virtual void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // No use simulating an empty circuit
            if (circuit.Objects.Count == 0)
                throw new CircuitException("{0}: No circuit objects for simulation".FormatString(Name));

            // Setup all objects
            circuit.Objects.BuildOrderedComponentList();
            foreach (var o in circuit.Objects)
            {
                o.Setup(this);
            }
            if (Nodes.Count < 1)
                throw new CircuitException("{0}: No circuit nodes for simulation".FormatString(Name));

            // Get all parameters
            SetupParameters(circuit.Objects);
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected abstract void Unsetup();

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
        protected Collection<T> SetupBehaviors<T>(IEnumerable<Entity> entities) where T : Behavior
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            // Register all behaviors
            foreach (var o in entities)
            {
                T behavior = o.CreateBehavior<T>(EntityParameters, EntityBehaviors);
                if (behavior != null)
                    EntityBehaviors.Add(o.Name, behavior);
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
            foreach (var o in entities)
            {
                foreach (var p in o.ParameterSets.Values)
                    EntityParameters.Add(o.Name, p);
            }
        }
    }
}
