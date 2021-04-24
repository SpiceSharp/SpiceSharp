using System;
using System.Runtime.Serialization;

namespace SpiceSharp
{
    /// <summary>
    /// An exception thrown when a subcircuit cannot be represented.
    /// </summary>
    /// <seealso cref="SpiceSharpException" /> 
    [Serializable]
    public class NoEquivalentSubcircuitException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context info.</param>
        protected NoEquivalentSubcircuitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEquivalentSubcircuitException"/> class.
        /// </summary>
        public NoEquivalentSubcircuitException()
            : base(Properties.Resources.Subcircuits_NoEquivalent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEquivalentSubcircuitException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public NoEquivalentSubcircuitException(Exception innerException)
            : base(Properties.Resources.Subcircuits_NoEquivalent, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEquivalentSubcircuitException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NoEquivalentSubcircuitException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEquivalentSubcircuitException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NoEquivalentSubcircuitException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
