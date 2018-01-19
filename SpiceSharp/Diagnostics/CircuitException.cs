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
        /// <param name="msg">Message</param>
        public CircuitException(string msg) : base(msg) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="innerException">Inner exception</param>
        public CircuitException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
