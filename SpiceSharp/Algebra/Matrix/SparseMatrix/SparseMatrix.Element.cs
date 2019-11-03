namespace SpiceSharp.Algebra
{
    public partial class SparseMatrix<T>
    {
        /// <summary>
        /// An element for a sparse matrix.
        /// </summary>
        /// <seealso cref="Element{T}" />
        /// <seealso cref="ISparseMatrixElement{T}"/>
        protected class Element : Element<T>, ISparseMatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the row index.
            /// </summary>
            /// <value>
            /// The row index.
            /// </value>
            public int Row { get; set; }

            /// <summary>
            /// Gets or sets the column index.
            /// </summary>
            /// <value>
            /// The column index.
            /// </value>
            public int Column { get; set; }

            /// <summary>
            /// Gets or sets the next element in the row.
            /// </summary>
            public Element Right { get; set; }

            /// <summary>
            /// Gets or sets the next element in the column.
            /// </summary>
            public Element Below { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the row.
            /// </summary>
            public Element Left { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the column.
            /// </summary>
            public Element Above { get; set; }

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
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Above => Above;

            /// <summary>
            /// Gets the matrix element below this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Below => Below;

            /// <summary>
            /// Gets the right.
            /// </summary>
            /// <value>
            /// The right.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Right => Right;

            /// <summary>
            /// Gets the left.
            /// </summary>
            /// <value>
            /// The left.
            /// </value>
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Left => Left;
        }
    }
}
