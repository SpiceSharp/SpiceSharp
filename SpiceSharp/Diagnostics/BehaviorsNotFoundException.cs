using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp
{
    /// <summary>
    /// An exception that is thrown when a problem occurs find behaviors.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class BehaviorsNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Gets the name of the behaviors.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected BehaviorsNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Name = info.GetString(nameof(Name));
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
            info.AddValue(nameof(Name), Name);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorsNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BehaviorsNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorsNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public BehaviorsNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorsNotFoundException"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior container.</param>
        /// <param name="message">The message.</param>
        public BehaviorsNotFoundException(string name, string message)
            : base(message)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorsNotFoundException"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior container.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BehaviorsNotFoundException(string name, string message, Exception innerException)
            : base(message, innerException)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorsNotFoundException"/> class.
        /// </summary>
        public BehaviorsNotFoundException()
        {
            Name = null;
        }
    }
}
