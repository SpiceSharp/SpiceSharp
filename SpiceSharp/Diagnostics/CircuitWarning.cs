using System;
using System.Collections.Generic;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// Keep track of warnings
    /// </summary>
    public class CircuitWarning
    {
        /// <summary>
        /// Keep track of all warnings
        /// </summary>
        private static List<string> warnings = new List<string>();

        /// <summary>
        /// Get a list of all warnings
        /// </summary>
        public static List<string> Warnings { get { return warnings; } }

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
            warnings.Add(msg);
            WarningGenerated?.Invoke(sender, arg);
        }
    }

    /// <summary>
    /// A class for warning events
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
    /// <param name="message"></param>
    public delegate void WarningEventHandler(object sender, WarningArgs e);
}
