using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for truncating the timestep
    /// </summary>
    public class TruncationEventArgs : EventArgs
    {
        /// <summary>
        /// The newly calculated delta
        /// This delta can only be made smaller using the setter.
        /// The getter will return min(2 * current delta, new delta)
        /// </summary>
        public double Delta
        {
            get => Math.Min(2.0 * CurrentDelta, delta);
            set => delta = Math.Min(value, delta);
        }
        double delta;

        /// <summary>
        /// Gets the current timestep
        /// </summary>
        public double CurrentDelta { get; }
        
        /// <summary>
        /// Get the simulation where the truncation event is called
        /// </summary>
        public TimeSimulation Simulation { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <param name="currentdelta">Initial timestep</param>
        public TruncationEventArgs(TimeSimulation simulation, double currentdelta)
        {
            Simulation = simulation;
            CurrentDelta = currentdelta;
            delta = double.PositiveInfinity;
        }
    }

    /// <summary>
    /// Event handler for when the timestep needs to be truncated
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="args">Arguments</param>
    public delegate void TruncationEventHandler(object sender, TruncationEventArgs args);
}
