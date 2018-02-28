using System;

namespace SpiceSharp.Algebra
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
        protected Vector(int length)
        {
            if (length < 1)
                throw new SparseException("Invalid vector length {0}".FormatString(length));
            Length = length;
        }

        /// <summary>
        /// Copy the vector contents to another vector
        /// </summary>
        /// <param name="target">Target</param>
        public void CopyTo(Vector<T> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Length != Length)
                throw new ArgumentException("Vector lengths do not match");
            for (int i = 1; i <= Length; i++)
                target[i] = this[i];
        }
    }
}
