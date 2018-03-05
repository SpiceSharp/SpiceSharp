using System;
using System.Globalization;
using System.Text;
using SpiceSharp.Algebra.Solve;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A system of linear equations
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class SparseLinearSystem<T> : IFormattable where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets the order of the matrix (matrix size)
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets whether or not the number of equations and variables is fixed
        /// </summary>
        public bool IsFixed { get; private set; }

        /// <summary>
        /// Gets the row translation
        /// </summary>
        protected Translation Row { get; } = new Translation();

        /// <summary>
        /// Gets the column translation
        /// </summary>
        protected Translation Column { get; } = new Translation();

        /// <summary>
        /// Gets the matrix to work on
        /// </summary>
        protected SparseMatrix<T> Matrix { get; }

        /// <summary>
        /// Gets the right-hand side vector
        /// </summary>
        protected SparseVector<T> Rhs { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SparseLinearSystem()
        {
            Matrix = new SparseMatrix<T>();
            Rhs = new SparseVector<T>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        public SparseLinearSystem(int size)
        {
            Matrix = new SparseMatrix<T>(size);
            Rhs = new SparseVector<T>(size);
        }

        /// <summary>
        /// Fix the number of equations and variables
        /// </summary>
        public virtual void FixEquations() => IsFixed = true;

        /// <summary>
        /// Unfix the number of equations and variables
        /// </summary>
        public virtual void UnfixEquations() => IsFixed = false;

        /// <summary>
        /// Get matrix element
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
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
        /// Find a matrix element
        /// Returns null if no element is found
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public MatrixElement<T> FindMatrixElement(int row, int column)
        {
            row = Row[row];
            column = Column[column];
            return Matrix.FindElement(row, column);
        }

        /// <summary>
        /// Get right-hand side element
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
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
        /// Gets the first row element in the reordered matrix
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public MatrixElement<T> FirstInReorderedRow(int row) => Matrix.GetFirstInRow(row);

        /// <summary>
        /// Gets the first column element in the reordered matrix
        /// </summary>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public MatrixElement<T> FirstInReorderedColumn(int column) => Matrix.GetFirstInColumn(column);

        /// <summary>
        /// Gets the diagonal element in the reordered matrix
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public MatrixElement<T> ReorderedDiagonal(int index) => Matrix.GetDiagonalElement(index);

        /// <summary>
        /// Gets the first element in the reordered Right-Hand Side vector
        /// </summary>
        /// <returns></returns>
        public VectorElement<T> FirstInReorderedRhs() => Rhs.First;

        /// <summary>
        /// Map external indices to internal indices
        /// </summary>
        /// <param name="externalIndexes">External indices</param>
        /// <returns></returns>
        public Tuple<int, int> ExternalToInternal(Tuple<int, int> externalIndexes)
        {
            if (externalIndexes == null)
                throw new ArgumentNullException(nameof(externalIndexes));

            int row = Row[externalIndexes.Item1];
            int column = Column[externalIndexes.Item2];
            return new Tuple<int, int>(row, column);
        }

        /// <summary>
        /// Map internal indices to external indices
        /// </summary>
        /// <param name="internalIndexes">Internal indices</param>
        /// <returns></returns>
        public Tuple<int, int> InternalToExternal(Tuple<int, int> internalIndexes)
        {
            if (internalIndexes == null)
                throw new ArgumentNullException(nameof(internalIndexes));

            int row = Row.Reverse(internalIndexes.Item1);
            int column = Column.Reverse(internalIndexes.Item2);
            return new Tuple<int, int>(row, column);
        }

        /// <summary>
        /// Swap (internal) rows in the system
        /// </summary>
        /// <param name="row1">Row 1</param>
        /// <param name="row2">Row 2</param>
        protected void SwapRows(int row1, int row2)
        {
            Matrix.SwapRows(row1, row2);
            Rhs.Swap(row1, row2);
            Row.Swap(row1, row2);
        }

        /// <summary>
        /// Swap (internal) columns in the system
        /// </summary>
        /// <param name="column1">Column 1</param>
        /// <param name="column2">Column 2</param>
        protected void SwapColumns(int column1, int column2)
        {
            Matrix.SwapColumns(column1, column2);
            Column.Swap(column1, column2);
        }

        /// <summary>
        /// Clear the matrix and right-hand side vector
        /// </summary>
        public virtual void Clear()
        {
            // Clear all matrix elements
            Matrix.FindElement(0, 0).Value = default;
            for (int r = 1; r <= Matrix.Size; r++)
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
                rhsElement = rhsElement.Next;
            }
        }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            // Build a matrix of strings for each element of the matrix
            string[][] displayData = new string[Matrix.Size][];
            int[] columnWidths = new int[Matrix.Size + 1];

            var rhsElement = Rhs.First;
            for (int r = 1; r <= Matrix.Size; r++)
            {
                var element = Matrix.GetFirstInRow(r);

                // Get the matching external row index
                int extRow = Row.Reverse(r) - 1;
                displayData[extRow] = new string[Matrix.Size + 1];

                for (int c = 1; c <= Matrix.Size; c++)
                {
                    // Get the matching external column index
                    int extColumn = Column.Reverse(c) - 1;

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
                    rhsElement = rhsElement.Next;

                // Show the element
                if (rhsElement != null && rhsElement.Index == r)
                    displayData[extRow][Matrix.Size] = rhsElement.Value.ToString(format, formatProvider);
                else
                    displayData[extRow][Matrix.Size] = "...";
                columnWidths[Matrix.Size] = Math.Max(columnWidths[Matrix.Size], displayData[extRow][Matrix.Size].Length);
            }

            // Build the string
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < Matrix.Size; r++)
            {
                for (int c = 0; c <= Matrix.Size; c++)
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
