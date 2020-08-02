using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A description of a vector.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IVector<T>
    {
        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        int Length { get; }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
        T this[int index] { get; set; }

        /// <summary>
        /// Swaps two elements in the vector.
        /// </summary>
        /// <param name="index1">The first index.</param>
        /// <param name="index2">The second index.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index1"/> or <paramref name="index2"/> is negative.</exception>
        void SwapElements(int index1, int index2);

        /// <summary>
        /// Copies the contents of the vector to another one.
        /// </summary>
        /// <param name="target">The target vector.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="target"/> does not have the same length as this vector.</exception>
        void CopyTo(IVector<T> target);

        /// <summary>
        /// Resets all elements in the vector to their default value.
        /// </summary>
        void Reset();

        /// <summary>
        /// Clears all elements in the vector. The size of the vector becomes 0.
        /// </summary>
        void Clear();
    }
}
