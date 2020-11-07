using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a vector that can be stepped through.
    /// </summary>
    public interface ISparseVector<T> : IVector<T>
    {
        /// <summary>
        /// Gets the number of elements in the vector.
        /// </summary>
        /// <value>
        /// The element count.
        /// </value>
        int ElementCount { get; }

        /// <summary>
        /// Gets the first <see cref="ISparseVectorElement{T}"/> in the vector.
        /// </summary>
        /// <returns>The first element in the vector.</returns>
        ISparseVectorElement<T> GetFirstInVector();

        /// <summary>
        /// Gets the last <see cref="ISparseVectorElement{T}"/> in the vector.
        /// </summary>
        /// <returns>
        /// The last element in the vector.
        /// </returns>
        ISparseVectorElement<T> GetLastInVector();

        /// <summary>
        /// Gets a vector element at the specified index. If
        /// it doesn't exist, a new one is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
        Element<T> GetElement(int index);

        /// <summary>
        /// Removes a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// <c>true</c> if the element was removed; otherwise, <c>false</c>.
        /// </returns>
        bool RemoveElement(int index);

        /// <summary>
        /// Finds a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
        Element<T> FindElement(int index);
    }
}
