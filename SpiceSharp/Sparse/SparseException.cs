using System;
using System.Runtime.Serialization;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Exception for Sparse matrix methods
    /// </summary>
    [Serializable]
    public class SparseException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SparseException() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public SparseException(string message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected SparseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
