using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Initialization
    /// </summary>
    public class InitializationDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets all behaviors
        /// </summary>
        public BehaviorPool Behaviors { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Behaviors</param>
        public InitializationDataEventArgs(BehaviorPool pool)
        {
            Behaviors = pool;
        }
    }
}
