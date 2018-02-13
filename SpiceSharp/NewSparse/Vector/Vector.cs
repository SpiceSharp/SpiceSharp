using System;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Base class for vectors
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class Vector<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets a value in the vector
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public abstract T this[int index] { get; set; }

        /// <summary>
        /// Gets the length of a vector
        /// </summary>
        public int Length { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public Vector(int length)
        {
            if (length < 1)
                throw new SparseException("Invalid vector length {0}".FormatString(length));
            Length = length;
        }
    }
}
