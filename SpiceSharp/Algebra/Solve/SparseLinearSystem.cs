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
    public abstract class SparseLinearSystem<T> : IFormattable where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets the order of the matrix (matrix size).
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets whether or not the number of equations and variables is fixed.
        /// </summary>
        public bool IsFixed { get; private set; }

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
        protected SparseMatrix<T> Matrix { get; }

        /// <summary>
        /// Gets the right-hand side vector.
        /// </summary>
        protected SparseVector<T> Rhs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseLinearSystem{T}"/> class.
        /// </summary>
        protected SparseLinearSystem()
        {
            Matrix = new SparseMatrix<T>();
            Rhs = new SparseVector<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseLinearSystem{T}"/> class.
        /// </summary>
        /// <param name="size">The number of equations and variables.</param>
        protected SparseLinearSystem(int size)
        {
            Matrix = new SparseMatrix<T>(size);
            Rhs = new SparseVector<T>(size);
        }

        /// <summary>
        /// Fix the number of equations and variables.
        /// </summary>
        /// <remarks>
        /// This method can be used to make sure that the matrix is fixed during
        /// solving. When fixed, it is impossible to add more elements to the sparse
        /// matrix or vector.
        /// </remarks>
        public virtual void FixEquations() => IsFixed = true;

        /// <summary>
        /// Unfix the number of equations and variables.
        /// </summary>
        public virtual void UnfixEquations() => IsFixed = false;

        /// <summary>
        /// Get a matrix element.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element.</returns>
        public MatrixElement<T> GetMatrixElement(int row, int column)
        {
            row = Row[row];
            column = Column[column];
            if (IsFixed)
            {
                if (row > Order || column > Order)
                    throw new SparseException("Linear system is fixed");
            }
            Order = Math.Max(Order, Math.Max(row, column));
            return Matrix.GetElement(row, column);
        }

        /// <summary>
        /// Find a matrix element.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element or null if there is none.</returns>
        public MatrixElement<T> FindMatrixElement(int row, int column)
        {
            row = Row[row];
            column = Column[column];
            return Matrix.FindElement(row, column);
        }

        /// <summary>
        /// Get the right-hand side vector element.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The vector element.</returns>
        public VectorElement<T> GetRhsElement(int index)
        {
            index = Row[index];
            if (IsFixed)
            {
                if (index > Order)
                    throw new SparseException("Linear system is fixed");
            }
            Order = Math.Max(index, Order);
            return Rhs.GetElement(index);
        }

        /// <summary>
        /// Finds a right-hand side vector element.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The vector element or null if there is none.</returns>
        public VectorElement<T> FindRhsElement(int index)
        {
            index = Row[index];
            return Rhs.FindElement(index);
        }

        /// <summary>
        /// Gets the first row element in the reordered matrix.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The first element in the row or null if there are none.</returns>
        public MatrixElement<T> FirstInReorderedRow(int row) => Matrix.GetFirstInRow(row);

        /// <summary>
        /// Gets the first column element in the reordered matrix.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The first element in the column or null if there are none.</returns>
        public MatrixElement<T> FirstInReorderedColumn(int column) => Matrix.GetFirstInColumn(column);

        /// <summary>
        /// Gets the diagonal element in the reordered matrix.
        /// </summary>
        /// <param name="index">The row/column of the diagonal element.</param>
        /// <returns>The first diagonal or null if there are none.</returns>
        public MatrixElement<T> ReorderedDiagonal(int index) => Matrix.GetDiagonalElement(index);

        /// <summary>
        /// Gets the first element in the reordered Right-Hand Side vector.
        /// </summary>
        /// <returns>The first element in the vector or null if there are none.</returns>
        public VectorElement<T> FirstInReorderedRhs() => Rhs.First;

        /// <summary>
        /// Transforms the indices from external to internal indices.
        /// </summary>
        /// <remarks>
        /// Opposite of <see cref="InternalToExternal(LinearSystemIndices)"/>.
        /// </remarks>
        /// <param name="indices">The row/column indices.</param>
        public void ExternalToInternal(LinearSystemIndices indices)
        {
            indices.Row = Row[indices.Row];
            indices.Column = Column[indices.Column];
        }

        /// <summary>
        /// Transforms the indices from internal to external indices.
        /// </summary>
        /// <remarks>
        /// Opposite of <see cref="ExternalToInternal(LinearSystemIndices)"/>.
        /// </remarks>
        /// <param name="indices">The row/column indices.</param>
        public void InternalToExternal(LinearSystemIndices indices)
        {
            indices.Row = Row.Reverse(indices.Row);
            indices.Column = Column.Reverse(indices.Column);
        }

        /// <summary>
        /// Swap two (internal) rows in the linear system.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        protected void SwapRows(int row1, int row2)
        {
            Matrix.SwapRows(row1, row2);
            Rhs.Swap(row1, row2);
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
        /// Clear the matrix and right-hand side vector.
        /// </summary>
        public virtual void Clear()
        {
            // Clear all matrix elements
            Matrix.FindElement(0, 0).Value = default;
            for (var r = 1; r <= Matrix.Size; r++)
            {
                var element = Matrix.GetFirstInRow(r);
                while (element != null)
                {
                    element.Value = default;
                    element = element.Right;
                }
            }

            // Clear all right-hand side elements
            var rhsElement = Rhs.First;
            while (rhsElement != null)
            {
                rhsElement.Value = default;
                rhsElement = rhsElement.Below;
            }
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
            // Build a matrix of strings for each element of the matrix
            var displayData = new string[Matrix.Size][];
            var columnWidths = new int[Matrix.Size + 1];

            var rhsElement = Rhs.First;
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
