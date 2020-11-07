using System;

namespace SpiceSharp.Algebra.Solve
{
    public abstract partial class SparseLUSolver<T>
    {
        /// <summary>
        /// A sparse matrix that keeps both the matrix and right hand side vector synchronized for our solver.
        /// </summary>
        /// <seealso cref="SparseLUSolver{T}" />
        protected class ReorderedMatrix : ISparseMatrix<T>
        {
            private readonly SparseLUSolver<T> _parent;

            /// <inheritdoc/>
            public int ElementCount => _parent.Matrix.ElementCount;

            /// <inheritdoc/>
            public int Size => _parent.Matrix.Size;

            /// <inheritdoc/>
            public T this[int row, int column]
            {
                get => _parent.Matrix[row, column];
                set => _parent.Matrix[row, column] = value;
            }

            /// <inheritdoc/>
            public T this[MatrixLocation location]
            {
                get => _parent.Matrix[location];
                set => _parent.Matrix[location] = value;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedMatrix"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public ReorderedMatrix(SparseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <inheritdoc/>
            public ISparseMatrixElement<T> GetFirstInRow(int row) => _parent.Matrix.GetFirstInRow(row);

            /// <inheritdoc/>
            public ISparseMatrixElement<T> GetLastInRow(int row) => _parent.Matrix.GetLastInRow(row);

            /// <inheritdoc/>
            public ISparseMatrixElement<T> GetFirstInColumn(int column) => _parent.Matrix.GetFirstInColumn(column);

            /// <inheritdoc/>
            public ISparseMatrixElement<T> GetLastInColumn(int column) => _parent.Matrix.GetLastInColumn(column);

            /// <inheritdoc/>
            public ISparseMatrixElement<T> FindDiagonalElement(int index) => _parent.Matrix.FindDiagonalElement(index);

            /// <inheritdoc/>
            public Element<T> GetElement(MatrixLocation location) => _parent.Matrix.GetElement(location);

            /// <inheritdoc/>
            public bool RemoveElement(MatrixLocation location) => _parent.Matrix.RemoveElement(location);

            /// <inheritdoc/>
            public Element<T> FindElement(MatrixLocation location) => _parent.Matrix.FindElement(location);

            /// <inheritdoc/>
            public void SwapRows(int row1, int row2) => _parent.SwapRows(row1, row2);

            /// <inheritdoc/>
            public void SwapColumns(int column1, int column2) => _parent.SwapColumns(column1, column2);

            /// <inheritdoc/>
            public void Reset() => _parent.Matrix.Reset();

            /// <inheritdoc/>
            public void Clear() => _parent.Matrix.Clear();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString() => _parent.Matrix.ToString();
        }
    }
}
