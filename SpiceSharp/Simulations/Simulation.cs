using System;
using System.Collections.ObjectModel;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

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
        public ParameterSetCollection Parameters { get; } = new ParameterSetCollection();

        /// <summary>
        /// States of the simulation
        /// </summary>
        public StateCollection States { get; } = new StateCollection();

        /// <summary>
        /// The circuit
        /// </summary>
        public Circuit Circuit { get; protected set; }

        /// <summary>
        /// Event that is called for initializing simulation data exports
        /// </summary>
        public event EventHandler<InitializationDataEventArgs> InitializeSimulationExport;

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event EventHandler<ExportDataEventArgs> OnExportSimulationData;

        /// <summary>
        /// Event that is called for finalizing simulation data exports
        /// </summary>
        public event FinalizeSimulationExportEventHandler FinalizeSimulationExport;

        /// <summary>
        /// Get the name of the simulation
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
        /// Run the simulation using a circuit
        /// </summary>
        /// <param name="circuit">Circuit</param>
        public virtual void Run(Circuit circuit)
        {
            // Store the circuit
            Circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));

            // Setup the simulation
            Setup();
            var args = new InitializationDataEventArgs(Pool);
            InitializeSimulationExport?.Invoke(this, args);

            // Execute the simulation
            Execute();

            // Finalize the simulation
            FinalizeSimulationExport?.Invoke(this, circuit);
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
        /// Create an export method
        /// If multiple behaviors implement the same property, the simulation type will decide which behavior gets precedence
        /// </summary>
        /// <param name="name">Entity name</param>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public abstract Func<RealState, double> CreateExport(Identifier name, string propertyName);

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
