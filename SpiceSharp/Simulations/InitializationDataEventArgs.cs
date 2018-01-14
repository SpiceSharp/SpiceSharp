using System;
using SpiceSharp.Behaviors;

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
    
    /// <summary>
    /// Delegate for initializing a simulation export
    /// </summary>
    /// <param name="sender">The object calling the event</param>
    /// <param name="args">Initialization data</param>
    public delegate void InitializeSimulationExportEventHandler(object sender, InitializationDataEventArgs args);
}
