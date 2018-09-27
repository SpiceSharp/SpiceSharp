using System;

namespace SpiceSharp
{
    /// <summary>
    /// An exception for circuit-related issues.
    /// </summary>
    /// <seealso cref="Exception" />
    public class CircuitException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitException"/> class.
        /// </summary>
        public CircuitException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CircuitException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public CircuitException(string message, Exception innerException) : base(message, innerException) { }
    }
}
