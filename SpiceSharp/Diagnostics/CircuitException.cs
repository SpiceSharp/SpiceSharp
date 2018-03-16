using System;
using System.Runtime.Serialization;

namespace SpiceSharp
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
        public CircuitException() { }

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected CircuitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
