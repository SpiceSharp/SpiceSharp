using System;

namespace SpiceSharp.Algebra.Solve
{
    public abstract partial class SparseLUSolver<T>
    {
        /// <summary>
        /// A sparse matrix that keeps everything synchronized for our solver.
        /// </summary>
        /// <seealso cref="SparseLUSolver{T}" />
        protected class ReorderedMatrix : ISparseMatrix<T>
        {
            private readonly SparseLUSolver<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedMatrix"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public ReorderedMatrix(SparseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Gets the number of elements in the matrix.
            /// </summary>
            /// <value>
            /// The element count.
            /// </value>
            public int ElementCount => _parent.Matrix.ElementCount;

            /// <summary>
            /// Gets the first non-default <see cref="ISparseMatrixElement{T}" /> in the specified row.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public ISparseMatrixElement<T> GetFirstInRow(int row) => _parent.Matrix.GetFirstInRow(row);

            /// <summary>
            /// Gets the last non-default <see cref="ISparseMatrixElement{T}" /> in the specified row.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public ISparseMatrixElement<T> GetLastInRow(int row) => _parent.Matrix.GetLastInRow(row);

            /// <summary>
            /// Gets the first non-default <see cref="ISparseMatrixElement{T}" /> in the specified column.
            /// </summary>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public ISparseMatrixElement<T> GetFirstInColumn(int column) => _parent.Matrix.GetFirstInColumn(column);

            /// <summary>
            /// Gets the last non-default <see cref="ISparseMatrixElement{T}" /> in the specified column.
            /// </summary>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public ISparseMatrixElement<T> GetLastInColumn(int column) => _parent.Matrix.GetLastInColumn(column);

            /// <summary>
            /// Finds the <see cref="ISparseMatrixElement{T}" /> on the diagonal.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            /// The matrix element if it exists; otherwise <c>null</c>.
            /// </returns>
            public ISparseMatrixElement<T> FindDiagonalElement(int index) => _parent.Matrix.FindDiagonalElement(index);

            /// <summary>
            /// Gets a pointer to the matrix element at the specified row and column. If
            /// the element doesn't exist, it is created.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public Element<T> GetElement(MatrixLocation location) => _parent.Matrix.GetElement(location);

            /// <summary>
            /// Finds a pointer to the matrix element at the specified row and column. If
            /// the element doesn't exist, <c>null</c> is returned.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element if it exists; otherwise <c>null</c>.
            /// </returns>
            public Element<T> FindElement(MatrixLocation location) => _parent.Matrix.FindElement(location);

            /// <summary>
            /// Gets the size of the matrix.
            /// </summary>
            /// <value>
            /// The matrix size.
            /// </value>
            public int Size => _parent.Matrix.Size;

            /// <summary>
            /// Gets or sets the value of the matrix at the specified row and column.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="row">The row.</param>
            /// <param name="column">The column.</param>
            /// <returns></returns>
            public T this[int row, int column]
            {
                get => _parent.Matrix[row, column];
                set => _parent.Matrix[row, column] = value;
            }

            /// <summary>
            /// Gets or sets the value of the matrix at the specified location.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="location">The location.</param>
            /// <returns></returns>
            public T this[MatrixLocation location]
            {
                get => _parent.Matrix[location];
                set => _parent.Matrix[location] = value;
            }

            /// <summary>
            /// Swaps two rows in the matrix.
            /// </summary>
            /// <param name="row1">The first row index.</param>
            /// <param name="row2">The second row index.</param>
            public void SwapRows(int row1, int row2) => _parent.SwapRows(row1, row2);

            /// <summary>
            /// Swaps two columns in the matrix.
            /// </summary>
            /// <param name="column1">The first column index.</param>
            /// <param name="column2">The second column index.</param>
            public void SwapColumns(int column1, int column2) => _parent.SwapColumns(column1, column2);

            /// <summary>
            /// Resets all elements in the matrix to their default value.
            /// </summary>
            public void Reset() => _parent.Matrix.Reset();

            /// <summary>
            /// Clears the matrix of any elements. The size of the matrix becomes 0.
            /// </summary>
            public void Clear() => _parent.Matrix.Clear();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString() => _parent.Matrix.ToString();
        }
    }
}
