using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a matrix that can be stepped through.
    /// </summary>
    /// <seealso cref="IElementMatrix{T}"/>
    public interface ISparseMatrix<T> : IElementMatrix<T> where T : IFormattable
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
        /// <returns></returns>
        new ISparseMatrixElement<T> FindDiagonalElement(int index);
    }
}
