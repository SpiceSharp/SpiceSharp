using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// Exception thrown when a parameter cannot be found.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class ParameterNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public virtual object ParameterizedObject { get; } = null;

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <value>
        /// The type of the parameter.
        /// </value>
        public virtual Type ParameterType { get; } = typeof(void);

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>
        /// The name of the parameter.
        /// </value>
        public string ParameterName { get; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ParameterNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ParameterName = info.GetString(nameof(ParameterName));
            ParameterType = Type.GetType(info.GetString(nameof(ParameterType)));

            // I guess this will do for now...
            ParameterizedObject = info.GetString(nameof(ParameterizedObject));
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
            info.AddValue(nameof(ParameterName), ParameterName);
            info.AddValue(nameof(ParameterType), ParameterType.FullName);

            // We don't know if the source is serializable, so we'll just send the string version
            info.AddValue(nameof(ParameterizedObject), ParameterizedObject?.ToString());
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public ParameterNotFoundException(object source, string name, Type type)
            : base(Properties.Resources.Parameters_NotFoundNamed.FormatString(name, type.Name, source))
        {
            ParameterizedObject = source;
            ParameterType = type;
            ParameterName = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="type">The type.</param>
        public ParameterNotFoundException(object source, Type type)
            : base(Properties.Resources.Parameters_NotFoundTyped.FormatString(type.Name, source))
        {
            ParameterizedObject = source;
            ParameterType = type;
            ParameterName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        public ParameterNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParameterNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ParameterNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
