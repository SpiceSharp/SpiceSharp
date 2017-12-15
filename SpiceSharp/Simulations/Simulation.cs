using System;
using System.Collections.Generic;
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
        /// The configuration
        /// </summary>
        protected SimulationConfiguration Config { get; set; } = null;

        /// <summary>
        /// The node that gives problems
        /// </summary>
        public CircuitNode ProblemNode { get; protected set; }

        /// <summary>
        /// The circuit
        /// </summary>
        public Circuit Circuit { get; protected set; }

        /// <summary>
        /// Get the current configuration (for use in the simulation)
        /// </summary>
        public SimulationConfiguration CurrentConfig => Config ?? SimulationConfiguration.Default;

        /// <summary>
        /// Event that is called for initializing simulation data exports
        /// </summary>
        public event InitializeSimulationExportEventHandler InitializeSimulationExport;

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event ExportSimulationDataEventHandler OnExportSimulationData;

        /// <summary>
        /// Event that is called for finalizing simulation data exports
        /// </summary>
        public event FinalizeSimulationExportEventHandler FinalizeSimulationExport;

        /// <summary>
        /// Get the name of the simulation
        /// </summary>
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration for this simulation</param>
        public Simulation(CircuitIdentifier name)
        {
            Config = null;
            Name = name;
        }

        /// <summary>
        /// Run the simulation using a circuit
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public virtual void Run(Circuit ckt)
        {
            // Store the circuit
            Circuit = ckt ?? throw new ArgumentNullException(nameof(ckt));

            // Setup the simulation
            Setup();
            InitializeSimulationExport?.Invoke(this, ckt);

            // Execute the simulation
            Execute();

            // Finalize the simulation
            FinalizeSimulationExport?.Invoke(this, ckt);
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
        /// <param name="ckt"></param>
        public virtual void Export(Circuit ckt)
        {
            SimulationData data = new SimulationData(ckt);
            OnExportSimulationData?.Invoke(this, data);
        }

        /// <summary>
        /// Collect behaviors of all circuit objects while also setting them up
        /// </summary>
        /// <typeparam name="T">Base behavior</typeparam>
        /// <returns></returns>
        protected List<T> SetupBehaviors<T>() where T : Behavior
        {
            List<T> result = new List<T>();
            foreach (var o in Circuit.Objects)
            {
                if (o.TryGetBehavior(typeof(T), out Behavior behavior))
                {
                    // The object returned a behavior, so we can setup and collect it
                    behavior.Setup(o, Circuit);
                    if (!behavior.DataOnly)
                        result.Add((T)behavior);
                }
            }
            return result;
        }
    }
}
