using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// An exception thrown if a component does not have a model but expects it.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class NoModelException : SpiceSharpException
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; } = "";

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <value>
        /// The type of the model.
        /// </value>
        public virtual Type ModelType { get; } = typeof(void);

        /// <summary>
        /// Initializes a new instance of the <see cref="NoModelException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected NoModelException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Name = info.GetString(nameof(Name));
            ModelType = Type.GetType(info.GetString(nameof(ModelType)));
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
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(ModelType), ModelType.FullName);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoModelException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="modelType">The expected model type.</param>
        public NoModelException(string name, Type modelType)
            : base(Properties.Resources.Components_NoModel.FormatString(name, modelType.Name))
        {
            Name = name;
            ModelType = modelType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoModelException"/> class.
        /// </summary>
        public NoModelException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoModelException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NoModelException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoModelException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NoModelException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
