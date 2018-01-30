using System;

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
        /// <param name="message">Message</param>
        public SparseException(string message) : base(message) { }
    }
}
