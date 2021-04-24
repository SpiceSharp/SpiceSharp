using System;
using System.Runtime.Serialization;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Sparse matrix exception
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class AlgebraException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context info.</param>
        protected AlgebraException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgebraException"/> class.
        /// </summary>
        public AlgebraException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgebraException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AlgebraException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgebraException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AlgebraException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
