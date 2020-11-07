using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// An <see cref="ISolver{T}"/> that uses sparse elements internally.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISolver{T}" />
    public interface ISparseSolver<T> : ISolver<T>
    {
        /// <summary>
        /// Finds the element at the specified location in the matrix.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        Element<T> FindElement(MatrixLocation location);

        /// <summary>
        /// Finds the element at the specified position in the right-hand side vector.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> is negative.</exception>
        Element<T> FindElement(int row);

        /// <summary>
        /// Gets the element at the specified location in the matrix. A new element is
        /// created if it doesn't exist yet.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        Element<T> GetElement(MatrixLocation location);

        /// <summary>
        /// Gets the element at the specified position in the right-hand side vector.
        /// A new element is created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> is negative.</exception>
        Element<T> GetElement(int row);

        /// <summary>
        /// Removes a matrix element at the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        /// <c>true</c> if the element was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool RemoveElement(MatrixLocation location);

        /// <summary>
        /// Removes a right-hand side vector element.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// <c>true</c> if the element was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool RemoveElement(int row);
    }
}
