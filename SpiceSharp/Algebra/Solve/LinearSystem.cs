using System;
using System.Globalization;
using System.Text;
using SpiceSharp.Algebra.Solve;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A class that represents a system of linear equations.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IFormattable" />
    public abstract partial class LinearSystem<M, V, T> : IFormattable 
        where M : IPermutableMatrix<T>
        where V : IPermutableVector<T>
        where T : IFormattable
    {
        /// <summary>
        /// Gets the order of the matrix (matrix size).
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Size => Math.Max(Matrix.Size, Vector.Length);

        /// <summary>
        /// Gets or sets the matrix value at the specified row and column.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value.</returns>
        public T this[int row, int column]
        {
            get
            {
                row = Row[row];
                column = Column[column];
                return Matrix[row, column];
            }
            set
            {
                row = Row[row];
                column = Column[column];
                Matrix[row, column] = value;
            }
        }

        /// <summary>
        /// Gets or sets the vector value at the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>Value</returns>
        public T this[int index]
        {
            get
            {
                index = Row[index];
                return Vector[index];
            }
            set
            {
                index = Row[index];
                Vector[index] = value;
            }
        }

        /// <summary>
        /// Gets the row translation.
        /// </summary>
        protected Translation Row { get; } = new Translation();

        /// <summary>
        /// Gets the column translation.
        /// </summary>
        protected Translation Column { get; } = new Translation();

        /// <summary>
        /// Gets the matrix to work on.
        /// </summary>
        protected M Matrix { get; }

        /// <summary>
        /// Gets the right-hand side vector.
        /// </summary>
        protected V Vector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSystem{M,V,T}"/> class.
        /// </summary>
        protected LinearSystem(M matrix, V vector)
        {
            Matrix = matrix;
            Vector = vector;
        }

        /// <summary>
        /// Swap two (internal) rows in the linear system. This method keeps
        /// the matrix and vector synchronized.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        protected void SwapRows(int row1, int row2)
        {
            Matrix.SwapRows(row1, row2);
            Vector.SwapElements(row1, row2);
            Row.Swap(row1, row2);
        }

        /// <summary>
        /// Swap two (internal) columns in the system.
        /// </summary>
        /// <param name="column1">The first column index.</param>
        /// <param name="column2">The second column index.</param>
        protected void SwapColumns(int column1, int column2)
        {
            Matrix.SwapColumns(column1, column2);
            Column.Swap(column1, column2);
        }

        /// <summary>
        /// Resets all elements in the matrix and vector.
        /// </summary>
        public void Reset()
        {
            ResetMatrix();
            ResetVector();
        }

        /// <summary>
        /// Resets all elements in the matrix.
        /// </summary>
        public virtual void ResetMatrix() => Matrix.Reset();

        /// <summary>
        /// Resets all elements in the vector.
        /// </summary>
        public virtual void ResetVector() => Vector.Reset();

        /// <summary>
        /// Maps an external row/column tuple to an internal one.
        /// </summary>
        /// <param name="indices">The external row/column indices.</param>
        /// <returns>
        /// The internal row/column indices.
        /// </returns>
        public MatrixLocation ExternalToInternal(MatrixLocation indices)
        {
            return new MatrixLocation(Row[indices.Row], Column[indices.Column]);
        }

        /// <summary>
        /// Maps an internal row/column tuple to an external one.
        /// </summary>
        /// <param name="indices">The internal row/column indices.</param>
        /// <returns>
        /// The external row/column indices.
        /// </returns>
        public MatrixLocation InternalToExternal(MatrixLocation indices)
        {
            return new MatrixLocation(Row.Reverse(indices.Row), Column.Reverse(indices.Column));
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            for (var r = 1; r <= Size; r++)
            {
                sb.Append("[ ");
                for (var c = 1; c <= Size; c++)
                {
                    sb.Append(this[r, c].ToString(format, formatProvider));
                    sb.Append(" ");
                }
                sb.Append(this[r].ToString(format, formatProvider));
                sb.Append(" ]");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
