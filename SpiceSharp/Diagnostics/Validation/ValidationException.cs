using System;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// An exception that is thrown for validation.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class ValidationException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
