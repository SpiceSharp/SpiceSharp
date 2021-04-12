using SpiceSharp.Validation;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An exception thrown when a simulation fails its validation.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class ValidationFailedException : SpiceSharpException
    {
        /// <summary>
        /// Gets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public virtual IRules Rules { get; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ValidationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
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

            // TODO: We may add serialization for rules here
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="rules">The rule provider.</param>
        public ValidationFailedException(ISimulation simulation, IRules rules)
            : base(Properties.Resources.Simulations_ValidationFailed.FormatString(simulation?.Name, rules?.ViolationCount))
        {
            Rules = rules;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="rules">The rule provider.</param>
        /// <param name="innerException">The inner exception.</param>
        public ValidationFailedException(ISimulation simulation, IRules rules, Exception innerException)
            : base(Properties.Resources.Simulations_ValidationFailed.FormatString(simulation?.Name, rules?.ViolationCount), innerException)
        {
            Rules = rules;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        public ValidationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ValidationFailedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ValidationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
