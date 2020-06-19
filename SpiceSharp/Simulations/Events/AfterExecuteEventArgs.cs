using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments that are used after simulation execution.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class AfterExecuteEventArgs : EventArgs
    {
        /// <summary>
        /// If <c>true</c>, the simulation will be requested to repeat the simulation another time.
        /// </summary>
        public bool Repeat { get; set; } = true;
    }
}
