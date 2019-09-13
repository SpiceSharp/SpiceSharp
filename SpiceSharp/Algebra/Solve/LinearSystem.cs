using System;
using System.Globalization;
using System.Text;
using SpiceSharp.Algebra.Solve;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A class that represents a system of linear equations.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IFormattable" />
    public abstract partial class LinearSystem<M, V, T> : IElementMatrix<T>, IElementVector<T>, IFormattable 
        where M : IPermutableMatrix<T>, IElementMatrix<T>
        where V : IPermutableVector<T>, IElementVector<T>
        where T : IFormattable
    {
        /// <summary>
        /// Gets the order of the matrix (matrix size).
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        int IVector<T>.Length => Vector.Length;

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
            Size = Math.Max(matrix.Size, vector.Length);
        }

        /// <summary>
        /// Gets the diagonal element at the specified row/column.
        /// </summary>
        /// <param name="index">The row/column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public IMatrixElement<T> FindDiagonalElement(int index)
        {
            int row = Row[index];
            int column = Column[index];
            return Matrix.FindMatrixElement(row, column);
        }

        /// <summary>
        /// Gets a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, it is created.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        /// <exception cref="SparseException">Linear system is fixed</exception>
        public IMatrixElement<T> GetMatrixElement(int row, int column)
        {
            row = Row[row];
            column = Column[column];
            Size = Math.Max(Math.Max(row, column), Size);
            return Matrix.GetMatrixElement(row, column);
        }

        /// <summary>
        /// Finds a pointer to the matrix element at the specified row and column. If
        /// the element doesn't exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element; otherwise <c>null</c>.
        /// </returns>
        public IMatrixElement<T> FindMatrixElement(int row, int column)
        {
            row = Row[row];
            column = Column[column];
            return Matrix.FindMatrixElement(row, column);
        }

        /// <summary>
        /// Gets a vector element at the specified index. If
        /// it doesn't exist, a new one is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        /// <exception cref="SparseException">Linear system is fixed</exception>
        public IVectorElement<T> GetVectorElement(int index)
        {
            if (Vector is IElementVector<T> v)
            {
                index = Row[index];
                return v.GetVectorElement(index);
            }
            return new ProxyVectorElement(this, index);
        }

        /// <summary>
        /// Finds a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element; otherwise <c>null</c>.
        /// </returns>
        public IVectorElement<T> FindVectorElement(int index)
        {
            if (Vector is IElementVector<T> v)
            {
                index = Row[index];
                return v.FindVectorElement(index);
            }
            return new ProxyVectorElement(this, index);
        }

        /// <summary>
        /// Copies the contents of the vector to another one.
        /// </summary>
        /// <param name="target">The target vector.</param>
        void IVector<T>.CopyTo(IVector<T> target)
        {
            Vector.CopyTo(target);
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
        public virtual void ResetMatrix() => Matrix.ResetMatrix();

        /// <summary>
        /// Resets all elements in the vector.
        /// </summary>
        public virtual void ResetVector() => Vector.ResetVector();

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
            for (var r = 1; r < Size; r++)
            {
                var row = Row[r];
                sb.Append("[ ");
                for (var c = 1; c < Size; c++)
                {
                    var column = Column[c];
                    sb.Append(Matrix[row, column].ToString(format, formatProvider));
                    sb.Append(" ");
                }
                sb.Append(Vector[row].ToString(format, formatProvider));
                sb.Append(" ]");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
