using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Sparse-related
    /// </summary>
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
    }
}
