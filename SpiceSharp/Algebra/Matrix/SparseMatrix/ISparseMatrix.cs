using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a matrix that can be stepped through.
    /// </summary>
    public interface ISparseMatrix<T> : IMatrix<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The matrix element.</returns>
        ISparseMatrixElement<T> GetFirstInRow(int row);

        /// <summary>
        /// Gets the last non-default <see cref="ISparseMatrixElement{T}"/> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The matrix element.</returns>
        ISparseMatrixElement<T> GetLastInRow(int row);

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        ISparseMatrixElement<T> GetFirstInColumn(int column);

        /// <summary>
        /// Gets the last non-default <see cref="ISparseMatrixElement{T}"/> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        ISparseMatrixElement<T> GetLastInColumn(int column);

        /// <summary>
        /// Finds the <see cref="ISparseMatrixElement{T}"/> on the diagonal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The matrix element if it exists; otherwise <c>null</c>.
        /// </returns>
        ISparseMatrixElement<T> FindDiagonalElement(int index);

        /// <summary>
        /// Gets a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, it is created.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        Element<T> GetElement(int row, int column);

        /// <summary>
        /// Finds a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element if it exists; otherwise <c>null</c>.
        /// </returns>
        Element<T> FindElement(int row, int column);
    }
}
