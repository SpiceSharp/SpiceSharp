using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when nodes aren't matched.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class NodeMismatchException : SpiceSharpException
    {
        /// <summary>
        /// Gets the expected number of nodes.
        /// </summary>
        /// <value>
        /// The expected number of nodes.
        /// </value>
        public virtual int Expected { get; } = -1;

        /// <summary>
        /// Gets the actual number of nodes.
        /// </summary>
        /// <value>
        /// The actual number of nodes.
        /// </value>
        public virtual int Actual { get; } = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected NodeMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Actual = info.GetInt32(nameof(Actual));
            Expected = info.GetInt32(nameof(Expected));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.ThrowIfNull(nameof(info));
            info.AddValue(nameof(Actual), Actual);
            info.AddValue(nameof(Expected), Expected);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        /// <param name="expected">The expected number of nodes.</param>
        /// <param name="actual">The actual number of nodes.</param>
        public NodeMismatchException(int expected, int actual)
            : base(Properties.Resources.Components_NodeMismatch.FormatString(expected, actual))
        {
            Expected = expected;
            Actual = actual;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        public NodeMismatchException(string name, int expected, int actual)
            : base(Properties.Resources.Components_NodeMismatchNamed.FormatString(name, expected, actual))
        {
            Expected = expected;
            Actual = actual;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        public NodeMismatchException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NodeMismatchException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NodeMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
