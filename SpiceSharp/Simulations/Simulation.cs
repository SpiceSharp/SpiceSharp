using System;
using System.Collections.ObjectModel;
using SpiceSharp.Behaviors;

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
        /// The circuit
        /// </summary>
        public Circuit Circuit { get; protected set; }

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
        /// Pool of all behaviors active in the simulation
        /// </summary>
        public BehaviorPool Behaviors { get; } = new BehaviorPool();

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
            // Store the circuit
            Circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));

            // Setup the simulation
            Setup();
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

            // Clear the circuit
            Circuit = null;
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected abstract void Setup();

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
        /// Collect behaviors of all circuit objects while also setting them up
        /// </summary>
        /// <typeparam name="T">Base behavior</typeparam>
        /// <returns></returns>
        protected Collection<T> SetupBehaviors<T>() where T : Behavior
        {
            // Register all behaviors
            foreach (var o in Circuit.Objects)
            {
                T behavior = o.GetBehavior<T>(Behaviors);
                if (behavior != null)
                    Behaviors.Add(o.Name, behavior);
            }
            return Behaviors.GetBehaviorList<T>();
        }
    }
}
