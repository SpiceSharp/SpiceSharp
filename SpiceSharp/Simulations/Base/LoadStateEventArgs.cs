using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Arguments when a state is loaded
    /// </summary>
    public class LoadStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state
        /// </summary>
        public State State { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        public LoadStateEventArgs(State state)
        {
            State = state;
        }
    }
}
