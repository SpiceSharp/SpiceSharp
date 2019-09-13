using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a matrix that can return elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IElementMatrix<T> : IMatrix<T> where T : IFormattable
    {
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
    }
}
