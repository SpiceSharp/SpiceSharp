using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharp
{
    /// <summary>
    /// A static class that tracks warnings.
    /// </summary>
    public static class CircuitWarning
    {
        // TODO: Shouldn't this also be thread-safe?

        /// <summary>
        /// Gets a collection of warnings.
        /// </summary>
        /// <value>
        /// The warnings.
        /// </value>
        public static ReadOnlyCollection<string> Warnings => WarningList.AsReadOnly();

        /// <summary>
        /// The list of warnings.
        /// </summary>
        private static readonly List<string> WarningList = new List<string>();

        /// <summary>
        /// Occurs when a warning was generated.
        /// </summary>
        public static event EventHandler<WarningEventArgs> WarningGenerated;

        /// <summary>
        /// Adds a warning.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The warning message.</param>
        public static void Warning(object sender, string message)
        {
            var arg = new WarningEventArgs(message);
            WarningList.Add(message);
            WarningGenerated?.Invoke(sender, arg);
        }
    }

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
