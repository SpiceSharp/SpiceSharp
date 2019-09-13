using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a matrix that has permutable rows and columns.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="IMatrix{T}" />
    public interface IPermutableMatrix<T> : IMatrix<T> where T : IFormattable
    {
        /// <summary>
        /// Occurs when two rows are swapped.
        /// </summary>
        event EventHandler<PermutationEventArgs> RowsSwapped;

        /// <summary>
        /// Occurs when two columns are swapped.
        /// </summary>
        event EventHandler<PermutationEventArgs> ColumnsSwapped;

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
    }
}
