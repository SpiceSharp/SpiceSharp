using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can perform a simulation using a <see cref="Circuit"/>.
    /// </summary>
    public abstract class Simulation<T> : Parameterized<T>, ISimulation
    {
        /// <summary>
        /// The configuration
        /// </summary>
        public SimulationConfiguration Config { get; set; } = null;

        public Circuit Circuit { get; set; }

        /// <summary>
        /// Get the current configuration (for use in the simulation)
        /// </summary>
        protected SimulationConfiguration CurrentConfig => Config ?? SimulationConfiguration.Default;

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
        public string Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration for this simulation</param>
        public Simulation(string name) : base()
        {
            Config = null;
            Name = name;
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// Setup and execute the simulation
        /// </summary>
        public void SetupAndExecute()
        {
            if (this.Circuit == null)
            {
                throw new CircuitException("No circuit for simulation");    
            }

            // Setup the circuit
            this.Circuit.Setup();

            if (this.Circuit.Objects.Count <= 0)
                throw new CircuitException("Circuit contains no objects");
            if (this.Circuit.Nodes.Count <= 1)
                throw new CircuitException("Circuit contains no nodes");

            // Do temperature-dependent calculations
            foreach (var c in this.Circuit.Objects)
                c.Temperature(this.Circuit);

            // Execute the simulation
            this.Execute();
        }

        /// <summary>
        /// Initialize the simulation
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void Initialize(Circuit ckt)
        {
            InitializeSimulationExport?.Invoke(this, ckt);
        }

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
        /// Finalize the simulation
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void Finalize(Circuit ckt)
        {
            FinalizeSimulationExport?.Invoke(this, ckt);
        }
    }
}
