namespace SpiceSharp.Algebra
{
    public partial class DenseMatrix<T>
    {
        /// <summary>
        /// A trash-can element for a <see cref="DenseMatrix{T}"/>.
        /// </summary>
        /// <seealso cref="SpiceSharp.Algebra.IMatrix{T}" />
        protected class TrashCanElement : IMatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the matrix element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value { get; set; }

            /// <summary>
            /// Gets the row.
            /// </summary>
            /// <value>
            /// The row.
            /// </value>
            public int Row => 0;

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>
            /// The column.
            /// </value>
            public int Column => 0;

            /// <summary>
            /// Gets the matrix element left of this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Left => null;

            /// <summary>
            /// Gets the matrix element right of this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Right => null;

            /// <summary>
            /// Gets the matrix element above this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Above => null;

            /// <summary>
            /// Gets the matrix element below this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Below => null;

            private DenseMatrix<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="TrashCanElement" /> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public TrashCanElement(DenseMatrix<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }
        }
    }
}
