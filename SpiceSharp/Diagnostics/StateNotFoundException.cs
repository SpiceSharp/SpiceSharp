using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp
{
    /// <summary>
    /// An exception that is thrown when a problem occurs find State.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class StateNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Gets the name of the State.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected StateNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            State = info.GetString(nameof(State));
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
            info.AddValue(nameof(State), State);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public StateNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public StateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotFoundException"/> class.
        /// </summary>
        /// <param name="state">The name of the state type.</param>
        /// <param name="message">The message.</param>
        public StateNotFoundException(string state, string message)
            : base(message)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotFoundException"/> class.
        /// </summary>
        /// <param name="state">The name of the state type.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StateNotFoundException(string state, string message, Exception innerException)
            : base(message, innerException)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotFoundException"/> class.
        /// </summary>
        public StateNotFoundException()
        {
            State = null;
        }
    }
}
