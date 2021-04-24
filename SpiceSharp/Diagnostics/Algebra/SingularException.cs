using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Exception thrown when a matrix is singular.
    /// </summary>
    /// <seealso cref="AlgebraException" />
    [Serializable]
    public class SingularException : AlgebraException
    {
        /// <summary>
        /// Gets the index where the first zero-diagonal element was found.
        /// </summary>
        public virtual int Index { get; } = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected SingularException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Index = info.GetInt32(nameof(Index));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.ThrowIfNull(nameof(info));
            info.AddValue(nameof(Index), Index);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        public SingularException()
            : base(Properties.Resources.Algebra_SingularMatrix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="index">The row/column index where the exception occurred.</param>
        public SingularException(int index)
            : base(Properties.Resources.Algebra_SingularMatrixIndexed.FormatString(index))
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SingularException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SingularException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
