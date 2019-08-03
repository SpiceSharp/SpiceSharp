using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments that are used before simulation execution.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class BeforeExecuteEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the simulation is repeated.
        /// </summary>
        public bool Repeated { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeExecuteEventArgs"/> class.
        /// </summary>
        /// <param name="repeated">if set to <c>true</c>, the simulation was repeated.</param>
        public BeforeExecuteEventArgs(bool repeated)
        {
            Repeated = repeated;
        }
    }
}
