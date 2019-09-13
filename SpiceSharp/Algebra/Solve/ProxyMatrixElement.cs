using System;

namespace SpiceSharp.Algebra
{
    public abstract partial class LinearSystem<M, V, T>
    {
        /// <summary>
        /// A matrix element that can be returned by <see cref="LinearSystem{T}"/>.
        /// </summary>
        /// <seealso cref="IElementMatrix{T}" />
        /// <seealso cref="IElementVector{T}" />
        /// <seealso cref="IFormattable" />
        protected class ProxyMatrixElement : IMatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the matrix element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value
            {
                get => _parent[Row, Column];
                set => _parent[Row, Column] = value;
            }

            /// <summary>
            /// Gets the row.
            /// </summary>
            /// <value>
            /// The row.
            /// </value>
            public int Row { get; }

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>
            /// The column.
            /// </value>
            public int Column { get; }

            private LinearSystem<M, V, T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyMatrixElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="row">The row.</param>
            /// <param name="column">The column.</param>
            public ProxyMatrixElement(LinearSystem<M, V, T> parent, int row, int column)
            {
                _parent = parent;
                Row = row;
                Column = column;
            }
        }
    }
}
