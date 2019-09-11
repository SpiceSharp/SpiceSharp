using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a matrix that is also permutable.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.IMatrix{T}" />
    public interface IPermutableMatrix<T> : IMatrix<T> where T : IFormattable
    {
        /// <summary>
        /// Swaps two rows in the matrix.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        void SwapRows(int row1, int row2);

        /// <summary>
        /// Swaps two columns in the matrix.
        /// </summary>
        /// <param name="column1">The first column index.</param>
        /// <param name="column2">The second column index.</param>
        void SwapColumns(int column1, int column2);

        /// <summary>
        /// Gets the first <see cref="IMatrixElement{T}"/> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The matrix element.</returns>
        IMatrixElement<T> GetFirstInRow(int row);

        /// <summary>
        /// Gets the last <see cref="IMatrixElement{T}"/> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The matrix element.</returns>
        IMatrixElement<T> GetLastInRow(int row);

        /// <summary>
        /// Gets the first <see cref="IMatrixElement{T}"/> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        IMatrixElement<T> GetFirstInColumn(int column);

        /// <summary>
        /// Gets the last <see cref="IMatrixElement{T}"/> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        IMatrixElement<T> GetLastInColumn(int column);
    }
}
