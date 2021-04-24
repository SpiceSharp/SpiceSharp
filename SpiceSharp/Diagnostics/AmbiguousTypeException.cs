using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp.General
{
    /// <summary>
    /// Exception for ambiguous types.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class AmbiguousTypeException : SpiceSharpException
    {
        /// <summary>
        /// Gets the type of the ambiguous.
        /// </summary>
        /// <value>
        /// The type of the ambiguous.
        /// </value>
        public virtual Type AmbiguousType { get; } = typeof(void);

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousTypeException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected AmbiguousTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AmbiguousType = Type.GetType(info.GetString("AmbiguousType"));
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
            info.AddValue(nameof(AmbiguousType), AmbiguousType.FullName);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public AmbiguousTypeException(Type type)
            : base(Properties.Resources.TypeDictionary_AmbiguousType.FormatString(type.FullName))
        {
            AmbiguousType = type ?? typeof(void);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousTypeException"/> class.
        /// </summary>
        public AmbiguousTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousTypeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AmbiguousTypeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousTypeException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AmbiguousTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
