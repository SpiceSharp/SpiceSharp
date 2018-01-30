using System;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// A basic exception for circuits
    /// </summary>
    [Serializable]
    public class CircuitException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public CircuitException(string message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        public CircuitException(string message, Exception innerException) : base(message, innerException) { }
    }
}
