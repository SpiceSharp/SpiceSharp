using System;
using System.Collections.Generic;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// Provides static methods for tracking warnings.
    /// </summary>
    public static class CircuitWarning
    {
        /// <summary>
        /// Get a list of all warnings
        /// </summary>
        public static List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// The event called when a warning is added
        /// </summary>
        public static event WarningEventHandler WarningGenerated;

        /// <summary>
        /// Add a warning
        /// </summary>
        /// <param name="msg"></param>
        public static void Warning(object sender, string msg)
        {
            WarningArgs arg = new WarningArgs(msg);
            Warnings.Add(msg);
            WarningGenerated?.Invoke(sender, arg);
        }
    }

    /// <summary>
    /// Warning arguments
    /// </summary>
    public class WarningArgs : EventArgs
    {
        /// <summary>
        /// Get the message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg"></param>
        public WarningArgs(string msg)
            : base()
        {
            Message = msg;
        }
    }

    /// <summary>
    /// A delegate for generating a warning
    /// </summary>
    /// <param name="sender">The object invoking the warning</param>
    /// <param name="message">The warning message</param>
    public delegate void WarningEventHandler(object sender, WarningArgs e);
}
