namespace SpiceSharp.Algebra
{
    public partial class SparseMatrix<T>
    {
        /// <summary>
        /// An element for a sparse matrix.
        /// </summary>
        /// <seealso cref="SpiceSharp.Algebra.Matrix{T}" />
        protected class SparseMatrixElement : MatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the row index.
            /// </summary>
            public new int Row
            {
                get => base.Row;
                set => base.Row = value;
            }

            /// <summary>
            /// Gets or sets the column index.
            /// </summary>
            public new int Column
            {
                get => base.Column;
                set => base.Column = value;
            }

            /// <summary>
            /// Gets or sets the next element in the row.
            /// </summary>
            public SparseMatrixElement NextInRow { get; set; }

            /// <summary>
            /// Gets or sets the next element in the column.
            /// </summary>
            public SparseMatrixElement NextInColumn { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the row.
            /// </summary>
            public SparseMatrixElement PreviousInRow { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the column.
            /// </summary>
            public SparseMatrixElement PreviousInColumn { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SparseMatrixElement"/> class.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            public SparseMatrixElement(int row, int column)
            {
                Value = default;
                Row = row;
                Column = column;
            }

            /// <summary>
            /// Gets the element above (same column).
            /// </summary>
            public override MatrixElement<T> Above => PreviousInColumn;

            /// <summary>
            /// Gets the element below (same column).
            /// </summary>
            public override MatrixElement<T> Below => NextInColumn;

            /// <summary>
            /// Gets the element on the right (same row).
            /// </summary>
            public override MatrixElement<T> Right => NextInRow;

            /// <summary>
            /// Gets the element on the left (same row).
            /// </summary>
            public override MatrixElement<T> Left => PreviousInRow;
        }
    }
}
