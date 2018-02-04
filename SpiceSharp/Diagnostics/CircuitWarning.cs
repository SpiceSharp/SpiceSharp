using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// Provides static methods for tracking warnings.
    /// </summary>
    public static class CircuitWarning
    {
        /// <summary>
        /// Gets a list of all warnings
        /// </summary>
        public static ReadOnlyCollection<string> Warnings { get => warnings.AsReadOnly(); }

        /// <summary>
        /// All warnings
        /// </summary>
        static List<string> warnings = new List<string>();

        /// <summary>
        /// The event called when a warning is added
        /// </summary>
        public static event EventHandler<WarningEventArgs> WarningGenerated;

        /// <summary>
        /// Add a warning
        /// </summary>
        /// <param name="message">Message</param>
        public static void Warning(object sender, string message)
        {
            WarningEventArgs arg = new WarningEventArgs(message);
            warnings.Add(message);
            WarningGenerated?.Invoke(sender, arg);
        }
    }

    /// <summary>
    /// Warning arguments
    /// </summary>
    public class WarningEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public WarningEventArgs(string message)
        {
            Message = message;
        }
    }
}
