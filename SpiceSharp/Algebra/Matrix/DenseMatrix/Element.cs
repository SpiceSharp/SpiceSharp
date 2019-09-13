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
            public IMatrixElement<T> Left
            {
                get
                {
                    var column = Column - 1;
                    while (column > 0 && _parent.GetMatrixValue(Row, column) == default)
                        column--;
                    if (column > 0)
                        return new Element(_parent, Row, column);
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
                    var column = Column + 1;
                    while (column <= _parent.Size && _parent.GetMatrixValue(Row, column) == default)
                        column++;
                    if (column <= _parent.Size)
                        return new Element(_parent, Row, column);
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
                    var row = Row - 1;
                    while (row > 0 && _parent.GetMatrixValue(row, Column) == default)
                        row--;
                    if (row > 0)
                        return new Element(_parent, row, Column);
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
                    var row = Row + 1;
                    while (row <= _parent.Size && _parent.GetMatrixValue(row, Column) == default)
                        row++;
                    if (row <= _parent.Size)
                        return new Element(_parent, row, Column);
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
