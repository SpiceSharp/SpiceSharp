using System;
using System.Collections.ObjectModel;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can perform a simulation using a <see cref="Circuit"/>.
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
        public event EventHandler<InitializeSimulationEventArgs> InitializeSimulationExport;

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event EventHandler<ExportDataEventArgs> OnExportSimulationData;

        /// <summary>
        /// Event that is called for finalizing simulation data exports
        /// </summary>
        public event EventHandler<FinalizeSimulationEventArgs> FinalizeSimulationExport;

        /// <summary>
        /// Gets the name of the simulation
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Pool of all behaviors in the simulation
        /// </summary>
        protected BehaviorPool Pool { get; } = new BehaviorPool();

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
            var initArgs = new InitializeSimulationEventArgs(Pool);
            InitializeSimulationExport?.Invoke(this, initArgs);

            // Execute the simulation
            if (controller != null)
            {
                controller.Initialize(this);
                do
                {
                    Execute();
                } while (controller.Continue(this));
                controller.Finalize(this);
            }
            else
               Execute();

            // Finalize the simulation
            var finalArgs = new FinalizeSimulationEventArgs();
            FinalizeSimulationExport?.Invoke(this, finalArgs);
            Unsetup();
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
                T behavior = o.GetBehavior<T>(Pool);
                if (behavior != null)
                    Pool.Add(o.Name, behavior);
            }
            return Pool.GetBehaviorList<T>();
        }
    }
}
