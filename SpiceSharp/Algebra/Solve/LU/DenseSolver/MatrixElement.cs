namespace SpiceSharp.Algebra
{
    public abstract partial class DenseLUSolver<M, V, T>
    {
        /// <summary>
        /// A <see cref="IMatrixElement{T}"/> returned by a <see cref="DenseLUSolver{M, V, T}"/>.
        /// </summary>
        /// <seealso cref="SpiceSharp.Algebra.LinearSystem{M, V, T}" />
        protected class MatrixElement : IMatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the matrix element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value
            {
                get => _parent.Matrix[Row, Column];
                set => _parent.Matrix[Row, Column] = value;
            }

            /// <summary>
            /// Gets the row.
            /// </summary>
            /// <value>
            /// The row.
            /// </value>
            public int Row => _parent.Row[_row];

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>
            /// The column.
            /// </value>
            public int Column => _parent.Column[_column];

            private DenseLUSolver<M, V, T> _parent;
            private int _row, _column;

            /// <summary>
            /// Initializes a new instance of the <see cref="MatrixElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="row">The row.</param>
            /// <param name="column">The column.</param>
            public MatrixElement(DenseLUSolver<M, V, T> parent, int row, int column)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _row = row;
                _column = column;
            }
        }
    }
}
