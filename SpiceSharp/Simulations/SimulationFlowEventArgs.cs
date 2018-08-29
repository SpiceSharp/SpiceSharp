using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments for determining the flow
    /// </summary>
    public class SimulationFlowEventArgs : EventArgs
    {
        /// <summary>
        /// If true, the simulation will be requested to repeat the simulation again
        /// </summary>
        public bool Repeat { get; set; } = true;
    }
}
