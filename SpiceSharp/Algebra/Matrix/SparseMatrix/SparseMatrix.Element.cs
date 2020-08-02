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
            /// <inheritdoc/>
            public int Row { get; set; }

            /// <inheritdoc/>
            public int Column { get; set; }

            /// <summary>
            /// Gets or sets the next element in the row.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public Element Right { get; set; }

            /// <summary>
            /// Gets or sets the next element in the column.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public Element Below { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the row.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public Element Left { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the column.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public Element Above { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Element"/> class.
            /// </summary>
            /// <param name="location">The location of the element.</param>
            public Element(MatrixLocation location)
            {
                Value = default;
                Row = location.Row;
                Column = location.Column;
            }

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

            ISparseMatrixElement<T> ISparseMatrixElement<T>.Above => Above;
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Below => Below;
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Right => Right;
            ISparseMatrixElement<T> ISparseMatrixElement<T>.Left => Left;
        }
    }
}
