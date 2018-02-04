using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Initialization
    /// </summary>
    public class InitializeSimulationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets all behaviors
        /// </summary>
        public BehaviorPool Behaviors { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Behaviors</param>
        public InitializeSimulationEventArgs(BehaviorPool pool)
        {
            Behaviors = pool;
        }
    }
}
