using System;

namespace SpiceSharp
{
    /// <summary>
    /// Event arguments that are used when a warning is generated.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class WarningEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the warning message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WarningEventArgs"/> class.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public WarningEventArgs(string message)
        {
            Message = message;
        }
    }
}
