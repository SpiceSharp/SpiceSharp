﻿using System;
#if !NETSTANDARD1_5
using System.Runtime.Serialization;
#endif

namespace SpiceSharp
{
    /// <summary>
    /// An exception for SpiceSharp-related issues.
    /// </summary>
    /// <seealso cref="Exception" />
#if !NETSTANDARD1_5
    [Serializable]
#endif
    public class SpiceSharpException : Exception
    {
#if !NETSTANDARD1_5
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context info.</param>
        protected SpiceSharpException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        public SpiceSharpException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SpiceSharpException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSharpException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SpiceSharpException(string message, Exception innerException) : base(message, innerException) { }
    }
}
