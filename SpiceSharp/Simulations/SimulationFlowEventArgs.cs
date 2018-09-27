using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments for determining whether or not another simulation should be executed.
    /// </summary>
    public class SimulationFlowEventArgs : EventArgs
    {
        /// <summary>
        /// If <c>true</c>, the simulation will be requested to repeat the simulation another time.
        /// </summary>
        public bool Repeat { get; set; } = true;
    }
}
