using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a matrix.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IMatrix<T>
    {
        /// <summary>
        /// Gets the size of the matrix.
        /// </summary>
        /// <value>
        /// The matrix size.
        /// </value>
        int Size { get; }

        /// <summary>
        /// Gets or sets the value at the specified row and column.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="row"/> or <paramref name="column"/> is not positive.
        /// </exception>
        T this[int row, int column] { get; set; }

        /// <summary>
        /// Gets or sets the value at the specified location.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="location">The location.</param>
        /// <returns>The value.</returns>
        T this[MatrixLocation location] { get; set; }

        /// <summary>
        /// Swaps two rows in the matrix.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="row1"/> or <paramref name="row2"/> is not greater than 0.
        /// </exception>
        void SwapRows(int row1, int row2);

        /// <summary>
        /// Swaps two columns in the matrix.
        /// </summary>
        /// <param name="column1">The first column index.</param>
        /// <param name="column2">The second column index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="column1"/> or <paramref name="column2"/> is not greater than 0.
        /// </exception>
        void SwapColumns(int column1, int column2);

        /// <summary>
        /// Resets all elements in the matrix to their default value.
        /// </summary>
        void Reset();

        /// <summary>
        /// Clears the matrix of any elements. The size of the matrix becomes 0.
        /// </summary>
        void Clear();
    }
}
