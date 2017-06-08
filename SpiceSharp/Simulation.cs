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
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration for this simulation</param>
        public Simulation(SimulationConfiguration config = null)
        {
            if (config == null)
                Config = new SimulationConfiguration();
            else
                Config = config;
        }
    }
}
