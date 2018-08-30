using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments before simulation execution
    /// </summary>
    public class BeforeExecuteEventArgs : EventArgs
    {
        /// <summary>
        /// False if the simulation is executed for the first time
        /// </summary>
        public bool Repeated { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repeated">Has the simulation already been repeated?</param>
        public BeforeExecuteEventArgs(bool repeated)
        {
            Repeated = repeated;
        }
    }
}
