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
        /// <param name="newDelta">New timestep</param>
        /// <param name="reason">Reason for cutting the timestep</param>
        public TimestepCutEventArgs(double newDelta, TimestepCutReason reason)
        {
            NewDelta = newDelta;
            Reason = reason;
        }
    }
}
