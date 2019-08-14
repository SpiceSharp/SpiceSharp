using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A base class for vectors.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class Vector<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <remarks>
        /// The element at index 0 is considered a trash can element. Use indices ranging 1 to the vector length.
        /// </remarks>
        /// <param name="index">The index in the vector.</param>
        /// <returns>The value at the specified index.</returns>
        public abstract T this[int index] { get; set; }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <remarks>
        /// Since the element at index 0 is considered a trash can element, the length also indicates the maximum index.
        /// </remarks>
        public int Length { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector{T}"/> class.
        /// </summary>
        /// <param name="length">The length of the vector.</param>
        protected Vector(int length)
        {
            if (length < 0)
                throw new SparseException("Invalid vector length {0}".FormatString(length));
            Length = length;
        }

        /// <summary>
        /// Copy the vector contents to another vector.
        /// </summary>
        /// <param name="target">The target vector.</param>
        public void CopyTo(Vector<T> target)
        {
            target.ThrowIfNull(nameof(target));
            if (target.Length != Length)
                throw new ArgumentException("Vector lengths do not match");
            for (var i = 1; i <= Length; i++)
                target[i] = this[i];
        }
    }
}
