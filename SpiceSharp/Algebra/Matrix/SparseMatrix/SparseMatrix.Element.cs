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
            /// Gets the row of the matrix element.
            /// </summary>
            /// <value>
            /// The row index.
            /// </value>
            public int Row { get; set; }

            /// <summary>
            /// Gets the column of the matrix element.
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
            /// <param name="row">The row.</param>
            /// <param name="column">The column.</param>
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
