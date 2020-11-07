using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a sparse matrix that return elements that have links to neighboring non-zero elements.
    /// </summary>
    /// <seealso cref="IMatrix{T}"/>
    public interface ISparseMatrix<T> : IMatrix<T>
    {
        /// <summary>
        /// Gets the number of elements in the matrix.
        /// </summary>
        /// <value>
        /// The element count.
        /// </value>
        int ElementCount { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}" /> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row"/> is negative.</exception>
        ISparseMatrixElement<T> GetFirstInRow(int row);

        /// <summary>
        /// Gets the last non-default <see cref="ISparseMatrixElement{T}" /> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row" /> is negative.</exception>
        ISparseMatrixElement<T> GetLastInRow(int row);

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="column"/> is negative.</exception>
        ISparseMatrixElement<T> GetFirstInColumn(int column);

        /// <summary>
        /// Gets the last non-default <see cref="ISparseMatrixElement{T}" /> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="column" /> is negative.</exception>
        ISparseMatrixElement<T> GetLastInColumn(int column);

        /// <summary>
        /// Finds the <see cref="ISparseMatrixElement{T}" /> on the diagonal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The matrix element if it exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index" /> is negative.</exception>
        ISparseMatrixElement<T> FindDiagonalElement(int index);

        /// <summary>
        /// Gets a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, it is created.
        /// </summary>
        /// <param name="location">The matrix location.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        Element<T> GetElement(MatrixLocation location);

        /// <summary>
        /// Finds a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="location">The matrix location.</param>
        /// <returns>
        /// The matrix element if it exists; otherwise <c>null</c>.
        /// </returns>
        Element<T> FindElement(MatrixLocation location);

        /// <summary>
        /// Removes a matrix element at the specified row and column. If the element
        /// doesn't exist, this method returns <c>false</c>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        /// <c>true</c> if the element was removed; otherwise, <c>false</c>.
        /// </returns>
        bool RemoveElement(MatrixLocation location);
    }
}
