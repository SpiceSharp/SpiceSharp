using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments that are used when a state is loaded.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class LoadStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public State State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadStateEventArgs"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public LoadStateEventArgs(State state)
        {
            State = state;
        }
    }
}
