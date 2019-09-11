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
    public abstract partial class LinearSystem<T> : IMatrix<T>, IVector<T>, IFormattable where T : IFormattable
    {
        /// <summary>
        /// Gets the order of the matrix (matrix size).
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        T IVector<T>.this[int index]
        {
            get => GetVectorValue(index);
            set => SetVectorValue(index, value);
        }

        /// <summary>
        /// Gets or sets the value with the specified row.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value.</returns>
        T IMatrix<T>.this[int row, int column]
        {
            get => GetMatrixValue(row, column);
            set => SetMatrixValue(row, column, value);
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        int IVector<T>.Length => Vector.Length;

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
        protected IPermutableMatrix<T> Matrix { get; }

        /// <summary>
        /// Gets the right-hand side vector.
        /// </summary>
        protected IPermutableVector<T> Vector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSystem{T}"/> class.
        /// </summary>
        protected LinearSystem()
        {
            Matrix = new SparseMatrix<T>();
            Vector = new SparseVector<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSystem{T}"/> class.
        /// </summary>
        /// <param name="size">The number of equations and variables.</param>
        protected LinearSystem(int size)
        {
            Matrix = new SparseMatrix<T>(size);
            Vector = new SparseVector<T>(size);
        }

        /// <summary>
        /// Gets the value in the matrix at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The value at the specified row and column.
        /// </returns>
        public T GetMatrixValue(int row, int column)
        {
            row = Row[row];
            column = Column[column];
            return Matrix.GetMatrixValue(row, column);
        }

        /// <summary>
        /// Sets the value in the matrix at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        public void SetMatrixValue(int row, int column, T value)
        {
            row = Row[row];
            column = Column[column];
            Matrix.SetMatrixValue(row, column, value);
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
            return Matrix.GetMatrixElement(row, column);
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
            Size = Math.Max(Size, Math.Max(row, column));
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
        /// Gets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public T GetVectorValue(int index)
        {
            index = Row[index];
            return Vector.GetVectorValue(index);
        }

        /// <summary>
        /// Sets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetVectorValue(int index, T value)
        {
            index = Row[index];
            Vector.SetVectorValue(index, value);
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
            index = Row[index];
            Size = Math.Max(index, Size);
            return Vector.GetVectorElement(index);
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
            index = Row[index];
            return Vector.FindVectorElement(index);
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
        /// Swap two (internal) rows in the linear system.
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
            // Build a matrix of strings for each element of the matrix
            var displayData = new string[Matrix.Size][];
            var columnWidths = new int[Matrix.Size + 1];

            var rhsElement = Vector.GetFirstInVector();
            for (var r = 1; r <= Matrix.Size; r++)
            {
                var element = Matrix.GetFirstInRow(r);

                // Get the matching external row index
                var extRow = Row.Reverse(r) - 1;
                displayData[extRow] = new string[Matrix.Size + 1];

                for (var c = 1; c <= Matrix.Size; c++)
                {
                    // Get the matching external column index
                    var extColumn = Column.Reverse(c) - 1;

                    // go to the next element if necessary
                    if (element != null && element.Column < c)
                        element = element.Right;

                    // Show the element
                    if (element == null || element.Column != c)
                        displayData[extRow][extColumn] = "...";
                    else
                        displayData[extRow][extColumn] = element.Value.ToString(format, formatProvider);
                    columnWidths[extColumn] = Math.Max(columnWidths[extColumn], displayData[extRow][extColumn].Length);
                }

                if (rhsElement != null && rhsElement.Index < r)
                    rhsElement = rhsElement.Below;

                // Show the element
                if (rhsElement != null && rhsElement.Index == r)
                    displayData[extRow][Matrix.Size] = rhsElement.Value.ToString(format, formatProvider);
                else
                    displayData[extRow][Matrix.Size] = "...";
                columnWidths[Matrix.Size] = Math.Max(columnWidths[Matrix.Size], displayData[extRow][Matrix.Size].Length);
            }

            // Build the string
            var sb = new StringBuilder();
            for (var r = 0; r < Matrix.Size; r++)
            {
                for (var c = 0; c <= Matrix.Size; c++)
                {
                    if (c == Matrix.Size)
                        sb.Append(" : ");

                    var display = displayData[r][c];
                    sb.Append(new string(' ', columnWidths[c] - display.Length + 2));
                    sb.Append(display);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
