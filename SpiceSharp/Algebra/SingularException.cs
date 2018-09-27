using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Exception thrown when a matrix is singular.
    /// </summary>
    /// <seealso cref="SparseException" />
    public class SingularException : SparseException
    {
        /// <summary>
        /// Gets the index where the first zero-diagonal element was found.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        public SingularException()
            : base("Singular matrix")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="index">The index where the exception occurred.</param>
        public SingularException(int index)
            : base("Singular matrix")
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SingularException(string message)
            : base(message)
        {
            Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="index">The index where the exception occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SingularException(int index, string message)
            : base(message)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SingularException(string message, Exception innerException)
            : base(message, innerException)
        {
            Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="index">The index where the exception occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SingularException(int index, string message, Exception innerException)
            : base(message, innerException)
        {
            Index = index;
        }
    }
}
