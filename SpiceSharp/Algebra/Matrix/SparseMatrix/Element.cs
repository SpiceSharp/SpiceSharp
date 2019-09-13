namespace SpiceSharp.Algebra
{
    public partial class SparseMatrix<T>
    {
        /// <summary>
        /// An element for a sparse matrix.
        /// </summary>
        /// <seealso cref="IMatrixElement{T}" />
        protected class Element : ISparseMatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the matrix element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value { get; set; }

            /// <summary>
            /// Gets or sets the row index.
            /// </summary>
            public int Row { get; set; }

            /// <summary>
            /// Gets or sets the column index.
            /// </summary>
            public int Column { get; set; }

            /// <summary>
            /// Gets or sets the next element in the row.
            /// </summary>
            public Element NextInRow { get; set; }

            /// <summary>
            /// Gets or sets the next element in the column.
            /// </summary>
            public Element NextInColumn { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the row.
            /// </summary>
            public Element PreviousInRow { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the column.
            /// </summary>
            public Element PreviousInColumn { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Element"/> class.
            /// </summary>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            public Element(int row, int column)
            {
                Value = default;
                Row = row;
                Column = column;
            }

            /// <summary>
            /// Gets the matrix element above this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Above => PreviousInColumn;

            /// <summary>
            /// Gets the matrix element below this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Below => NextInColumn;

            /// <summary>
            /// Gets the right.
            /// </summary>
            /// <value>
            /// The right.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Right => NextInRow;

            /// <summary>
            /// Gets the left.
            /// </summary>
            /// <value>
            /// The left.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Left => PreviousInRow;
        }
    }
}
