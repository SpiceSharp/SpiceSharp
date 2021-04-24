using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp
{
    /// <summary>
    /// An exception that is thrown when a problem occurs with the generic type of a method.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class TypeNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Gets the type of the generic.
        /// </summary>
        /// <value>
        /// The type of the generic.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected TypeNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Type = Type.GetType(info.GetString(nameof(Type)));
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
            info.AddValue(nameof(Type), Type.FullName);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TypeNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public TypeNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="message">The message.</param>
        public TypeNotFoundException(Type type, string message)
            : base(message)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TypeNotFoundException(Type type, string message, Exception innerException)
            : base(message, innerException)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        public TypeNotFoundException()
        {
            Type = null;
        }
    }
}
