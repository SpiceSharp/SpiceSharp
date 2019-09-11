using System;

namespace SpiceSharp.Algebra
{
    public abstract partial class LinearSystem<T>
    {
        /// <summary>
        /// This class can be used by <see cref="LinearSystem{T}"/> for preconditioning
        /// the Y-matrix. If the preconditioner decides to swap rows/columns, this class
        /// will make sure that the translation is tracked.
        /// </summary>
        /// <seealso cref="IPermutableMatrix{T}" />
        protected class LinearSystemMatrix : IPermutableMatrix<T>
        {
            private LinearSystem<T> _parent;

            /// <summary>
            /// Gets the size of the matrix.
            /// </summary>
            /// <value>
            /// The matrix size.
            /// </value>
            public int Size => _parent.Size;

            /// <summary>
            /// Gets or sets the value with the specified row.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <returns>The value</returns>
            public T this[int row, int column]
            {
                get => GetMatrixValue(row, column);
                set => SetMatrixValue(row, column, value);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LinearSystemMatrix"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public LinearSystemMatrix(LinearSystem<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Gets the diagonal element at the specified row/column.
            /// </summary>
            /// <param name="index">The row/column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public IMatrixElement<T> FindDiagonalElement(int index) => _parent.Matrix.FindDiagonalElement(index);

            /// <summary>
            /// Finds a pointer to the matrix element at the specified row and column. If
            /// the element doesn't exist, <c>null</c> is returned.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element; otherwise <c>null</c>.
            /// </returns>
            public IMatrixElement<T> FindMatrixElement(int row, int column) => _parent.Matrix.FindMatrixElement(row, column);

            /// <summary>
            /// Gets the first <see cref="IMatrixElement{T}" /> in the specified column.
            /// </summary>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public IMatrixElement<T> GetFirstInColumn(int column) => _parent.Matrix.GetFirstInColumn(column);

            /// <summary>
            /// Gets the first <see cref="IMatrixElement{T}" /> in the specified row.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public IMatrixElement<T> GetFirstInRow(int row) => _parent.Matrix.GetFirstInRow(row);

            /// <summary>
            /// Gets the last <see cref="IMatrixElement{T}" /> in the specified column.
            /// </summary>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public IMatrixElement<T> GetLastInColumn(int column) => _parent.Matrix.GetLastInColumn(column);

            /// <summary>
            /// Gets the last <see cref="IMatrixElement{T}" /> in the specified row.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public IMatrixElement<T> GetLastInRow(int row) => _parent.Matrix.GetLastInRow(row);

            /// <summary>
            /// Gets a pointer to the matrix element at the specified row and column. If
            /// the element doesn't exist, it is created.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The matrix element.
            /// </returns>
            public IMatrixElement<T> GetMatrixElement(int row, int column) => _parent.Matrix.GetMatrixElement(row, column);

            /// <summary>
            /// Gets the value in the matrix at the specified row and column.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <returns>
            /// The value at the specified row and column.
            /// </returns>
            public T GetMatrixValue(int row, int column) => _parent.Matrix.GetMatrixValue(row, column);

            /// <summary>
            /// Sets the value in the matrix at the specified row and column.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            /// <param name="value">The value.</param>
            public void SetMatrixValue(int row, int column, T value) => _parent.Matrix.SetMatrixValue(row, column, value);

            /// <summary>
            /// Swaps two columns in the matrix.
            /// </summary>
            /// <param name="column1">The first column index.</param>
            /// <param name="column2">The second column index.</param>
            public void SwapColumns(int column1, int column2)
            {
                _parent.SwapColumns(column1, column2);
            }

            /// <summary>
            /// Swaps two rows in the matrix.
            /// </summary>
            /// <param name="row1">The first row index.</param>
            /// <param name="row2">The second row index.</param>
            /// <exception cref="NotImplementedException"></exception>
            public void SwapRows(int row1, int row2)
            {
                _parent.SwapRows(row1, row2);
            }

            /// <summary>
            /// Resets all elements in the matrix.
            /// </summary>
            public void ResetMatrix() => _parent.Matrix.ResetMatrix();
        }
    }
}
