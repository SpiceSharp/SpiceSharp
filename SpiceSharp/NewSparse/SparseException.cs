using System;
using System.Runtime.Serialization;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Sparse-related
    /// </summary>
    [Serializable]
    public class SparseException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SparseException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public SparseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        public SparseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SparseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
