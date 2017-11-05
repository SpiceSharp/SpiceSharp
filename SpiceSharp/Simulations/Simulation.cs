using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Behaviors;

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
        protected SimulationConfiguration Config { get; set; } = null;

        public Circuit Circuit { get; set; }

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
            if (Circuit == null)
            {
                throw new CircuitException("No circuit for simulation");    
            }

            // Setup the circuit
            Circuit.Setup();

            if (Circuit.Objects.Count <= 0)
                throw new CircuitException("Circuit contains no objects");
            if (Circuit.Nodes.Count < 1)
                throw new CircuitException("Circuit contains no nodes");

            // Do temperature-dependent calculations
            var temperaturebehaviours = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorTemperature>(Circuit);
            foreach (var behaviour in temperaturebehaviours)
                behaviour.Execute(Circuit);

            // Execute the simulation
            Execute();
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
