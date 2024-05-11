using System;
using SpiceSharp.Simulations.Base;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// An exception that is thrown when a problem occurs trying to find a variable.
    /// </summary>
    [Serializable]
    public class VariableNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Gets the path that could not be found.
        /// </summary>
        public Reference Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/>.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected VariableNotFoundException(SerializationInfo info, StreamingContext context)
        {
            int length = info.GetInt32("path_length");
            string[] path = new string[length];
            for (int i = 0; i < length; i++)
                info.GetString($"path_{i}");
            Path = new(path);
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
            info.AddValue("path_length", Path.Length);
            for (int i = 0; i < Path.Length; i++)
                info.AddValue($"path_{i}", Path[i]);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VariableNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public VariableNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="path">The hierarchical reference.</param>
        /// <param name="message">The message.</param>
        public VariableNotFoundException(Reference path, string message)
            : base(message)
        {
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="path">The hierarchical reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public VariableNotFoundException(Reference path, string message, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        public VariableNotFoundException()
        {
            Path = default;
        }
    }
}
