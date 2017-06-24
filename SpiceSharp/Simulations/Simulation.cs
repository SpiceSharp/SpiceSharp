using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using MathNet.Numerics.LinearAlgebra;
using SpiceSharp.Circuits;

namespace SpiceSharp
{
    /// <summary>
    /// A class with everything needed to do a simulation
    /// </summary>
    public abstract class Simulation
    {
        /// <summary>
        /// The configuration
        /// </summary>
        public SimulationConfiguration Config { get; protected set; } = null;

        /// <summary>
        /// Gets the name of the simulation
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event ExportSimulationDataEventHandler ExportSimulationData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration for this simulation</param>
        public Simulation(string name, SimulationConfiguration config = null)
        {
            if (config == null)
                Config = new SimulationConfiguration();
            else
                Config = config;
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        /// <param name="ckt">The circuit to be used</param>
        /// <param name="reset">Restart the simulation when true</param>
        public abstract void Execute(Circuit ckt);

        /// <summary>
        /// Export the data
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void Export(Circuit ckt)
        {
            SimulationData data = new SimulationData(ckt);
            ExportSimulationData?.Invoke(this, data);
        }
    }
}
