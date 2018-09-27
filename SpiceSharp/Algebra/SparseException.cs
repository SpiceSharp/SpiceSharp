using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Sparse matrix exception
    /// </summary>
    /// <seealso cref="Exception" />
    public class SparseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SparseException"/> class.
        /// </summary>
        public SparseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SparseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SparseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
