using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a sparse matrix.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IMatrix<T> where T : IFormattable
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
        T this[int row, int column] { get; set; }

        /// <summary>
        /// Gets the value in the matrix at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The value at the specified row and column.
        /// </returns>
        T GetMatrixValue(int row, int column);

        /// <summary>
        /// Sets the value in the matrix at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        void SetMatrixValue(int row, int column, T value);

        /// <summary>
        /// Gets a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, it is created.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        IMatrixElement<T> GetMatrixElement(int row, int column);

        /// <summary>
        /// Finds a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element; otherwise <c>null</c>.
        /// </returns>
        IMatrixElement<T> FindMatrixElement(int row, int column);

        /// <summary>
        /// Gets the diagonal element at the specified row/column.
        /// </summary>
        /// <param name="index">The row/column index.</param>
        /// <returns>The matrix element.</returns>
        IMatrixElement<T> FindDiagonalElement(int index);

        /// <summary>
        /// Resets all elements in the matrix.
        /// </summary>
        void ResetMatrix();
    }
}
