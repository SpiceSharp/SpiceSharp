using System;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Biasing event arguments.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class BiasingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the task.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public int Task { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingEventArgs"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        public BiasingEventArgs(int task)
        {
            Task = task;
        }
    }
}
