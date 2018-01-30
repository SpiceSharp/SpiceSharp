using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class contains all data when a timeStep cut event is triggered
    /// </summary>
    public class TimeStepCutEventArgs : EventArgs
    {
        /// <summary>
        /// Enumerations
        /// </summary>
        public enum TimeStepCutReason
        {
            Convergence, // Cut due to convergence problems
            Truncation // Cut due to the local truncation error
        }

        /// <summary>
        /// Get the circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// The new timeStep that will be tried
        /// </summary>
        public double NewDelta { get; }

        /// <summary>
        /// Gets the reason for cutting the timeStep
        /// </summary>
        public TimeStepCutReason Reason { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circuit"></param>
        /// <param name="newDelta"></param>
        public TimeStepCutEventArgs(Circuit circuit, double newDelta, TimeStepCutReason reason)
        {
            Circuit = circuit;
            NewDelta = newDelta;
            Reason = reason;
        }
    }
}
