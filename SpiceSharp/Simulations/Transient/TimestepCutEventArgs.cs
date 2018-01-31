using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class contains all data when a timestep cut event is triggered
    /// </summary>
    public class TimestepCutEventArgs : EventArgs
    {
        /// <summary>
        /// Enumerations
        /// </summary>
        public enum TimestepCutReason
        {
            Convergence, // Cut due to convergence problems
            Truncation // Cut due to the local truncation error
        }

        /// <summary>
        /// Get the circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// The new timestep that will be tried
        /// </summary>
        public double NewDelta { get; }

        /// <summary>
        /// Gets the reason for cutting the timestep
        /// </summary>
        public TimestepCutReason Reason { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circuit"></param>
        /// <param name="newDelta"></param>
        public TimestepCutEventArgs(Circuit circuit, double newDelta, TimestepCutReason reason)
        {
            Circuit = circuit;
            NewDelta = newDelta;
            Reason = reason;
        }
    }
}
