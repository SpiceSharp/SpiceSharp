using System;

namespace SpiceSharp.Algebra
{
    public partial class DenseMatrix<T>
    {
        /// <summary>
        /// A matrix element for a <see cref="DenseMatrix{T}"/>.
        /// </summary>
        /// <seealso cref="SpiceSharp.Algebra.IMatrix{T}" />
        protected class Element : IMatrixElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the matrix element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value
            {
                get => _parent.GetMatrixValue(Row, Column);
                set => _parent.SetMatrixValue(Row, Column, value);
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

            /// <summary>
            /// Gets the matrix element left of this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            /// <exception cref="NotImplementedException"></exception>
            public IMatrixElement<T> Left
            {
                get
                {
                    if (Column > 1)
                        return new Element(_parent, Row, Column - 1);
                    return null;
                }
            }

            /// <summary>
            /// Gets the matrix element right of this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Right
            {
                get
                {
                    if (Column < _parent.Size)
                        return new Element(_parent, Row, Column + 1);
                    return null;
                }
            }

            /// <summary>
            /// Gets the matrix element above this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Above
            {
                get
                {
                    if (Row > 1)
                        return new Element(_parent, Row - 1, Column);
                    return null;
                }
            }

            /// <summary>
            /// Gets the matrix element below this one.
            /// </summary>
            /// <value>
            /// The matrix element.
            /// </value>
            public IMatrixElement<T> Below
            {
                get
                {
                    if (Row < _parent.Size)
                        return new Element(_parent, Row + 1, Column);
                    return null;
                }
            }

            private DenseMatrix<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="Element"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="row">The row index.</param>
            /// <param name="column">The column index.</param>
            public Element(DenseMatrix<T> parent, int row, int column)
            {
                Row = row;
                Column = column;
                _parent = parent.ThrowIfNull(nameof(parent));
            }
        }
    }
}
