using System;

namespace SpiceSharp
{
    /// <summary>
    /// A static class that tracks warnings.
    /// </summary>
    public static class SpiceSharpWarning
    {
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
            WarningGenerated?.Invoke(sender, arg);
        }
    }

}
