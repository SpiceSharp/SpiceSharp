using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Exception for Sparse matrix methods
    /// </summary>
    public class SparseException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Message</param>
        public SparseException(string msg) : base(msg) { }
    }
}
