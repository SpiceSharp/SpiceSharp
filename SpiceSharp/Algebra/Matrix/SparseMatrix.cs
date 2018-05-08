using System;
using System.Globalization;
using System.Text;
using SpiceSharp.Algebra.Matrix;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Matrix using a sparse representation
    /// The matrix is always kept square!
    /// </summary>
    /// <typeparam name="T">Type for the element</typeparam>
    public class SparseMatrix<T> : Matrix<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const int InitialSize = 4;
        private const float ExpansionFactor = 1.5f;

        /// <summary>
        /// Private variables
        /// </summary>
        private Row<T>[] _rows;
        private Column<T>[] _columns;
        private SparseMatrixElement<T>[] _diagonal;
        private readonly SparseMatrixElement<T> _trashCan;
        private int _allocatedSize;

        /// <summary>
        /// Constructor
        /// </summary>
        public SparseMatrix()
            : base(1)
        {
            _allocatedSize = InitialSize;

            // Allocate rows
            _rows = new Row<T>[InitialSize + 1];
            for (int i = 1; i <= InitialSize; i++)
                _rows[i] = new Row<T>();

            // Allocate columns
            _columns = new Column<T>[InitialSize + 1];
            for (int i = 1; i <= InitialSize; i++)
                _columns[i] = new Column<T>();

            // Other
            _diagonal = new SparseMatrixElement<T>[InitialSize + 1];
            _trashCan = new SparseMatrixElement<T>(0, 0);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        public SparseMatrix(int size)
            : base(size)
        {
            _allocatedSize = Math.Max(InitialSize, size);

            // Allocate rows
            _rows = new Row<T>[_allocatedSize + 1];
            for (int i = 1; i <= _allocatedSize; i++)
                _rows[i] = new Row<T>();

            // Allocate columns
            _columns = new Column<T>[_allocatedSize + 1];
            for (int i = 1; i <= _allocatedSize; i++)
                _columns[i] = new Column<T>();

            // Other
            _diagonal = new SparseMatrixElement<T>[_allocatedSize + 1];
            _trashCan = new SparseMatrixElement<T>(0, 0);
        }

        /// <summary>
        /// Gets a value in the matrix at a specific row and column
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public override T GetValue(int row, int column)
        {
            var element = FindElement(row, column);
            if (element == null)
                return default(T);
            return element.Value;
        }

        /// <summary>
        /// Sets the value in the matrix at a specific row and column
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public override void SetValue(int row, int column, T value)
        {
            if (value.Equals(default(T)))
            {
                // We don't need to create a new element unnecessarily
                var element = FindElement(row, column);
                if (element != null)
                    element.Value = default(T);
            }
            else
            {
                // We have to create an element if it doesn't exist yet
                var element = GetElement(row, column);
                element.Value = value;
            }
        }


        /// <summary>
        /// Get an element
        /// Creates a new element if it doesn't exist
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public MatrixElement<T> GetElement(int row, int column)
        {
            if (row < 0 || column < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));
            if (row == 0 || column == 0)
                return _trashCan;

            // Expand our matrix if it is necessary!
            if (row > Size || column > Size)
                ExpandMatrix(Math.Max(row, column));

            // Quick access to diagonals
            if (row == column && _diagonal[row] != null)
                return _diagonal[row];

            if (!_rows[row].CreateGetElement(row, column, out var element))
            {
                _columns[column].Insert(element);
                if (row == column)
                    _diagonal[row] = element;
            }

            return element;
        }

        /// <summary>
        /// Get a diagonal element
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public MatrixElement<T> GetDiagonalElement(int index) => _diagonal[index];
        
        /// <summary>
        /// Find an element
        /// Will not create a new element if it doesn't exist
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public MatrixElement<T> FindElement(int row, int column)
        {
            if (row < 0 || column < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));
            if (row > Size || column > Size)
                return null;
            if (row == 0 || column == 0)
                return _trashCan;

            // Find the element
            return _rows[row].Find(column);
        }

        /// <summary>
        /// Gets the first element in a row
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public MatrixElement<T> GetFirstInRow(int row) => _rows[row].FirstInRow;

        /// <summary>
        /// Gets the last element in a row
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public MatrixElement<T> GetLastInRow(int row) => _rows[row].LastInRow;

        /// <summary>
        /// Gets the first element in a column
        /// </summary>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public MatrixElement<T> GetFirstInColumn(int column) => _columns[column].FirstInColumn;

        /// <summary>
        /// Gets the last element in a column
        /// </summary>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public MatrixElement<T> GetLastInColumn(int column) => _columns[column].LastInColumn;

        /// <summary>
        /// Swap rows in the matrix
        /// </summary>
        /// <param name="row1">Row 1</param>
        /// <param name="row2">Row 2</param>
        public void SwapRows(int row1, int row2)
        {
            if (row1 < 0 || row2 < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row1, row2));
            if (row1 == row2)
                return;
            if (row2 < row1)
            {
                var tmp = row1;
                row1 = row2;
                row2 = tmp;
            }

            // Get the two elements
            SparseMatrixElement<T> row1Element = _rows[row1].FirstInRow;
            SparseMatrixElement<T> row2Element = _rows[row2].FirstInRow;

            // Swap the two rows
            var tmpRow = _rows[row1];
            _rows[row1] = _rows[row2];
            _rows[row2] = tmpRow;

            // Reset the diagonal elements
            _diagonal[row1] = null;
            _diagonal[row2] = null;

            // Swap the elements from left to right
            while (row1Element != null || row2Element != null)
            {
                if (row2Element == null)
                {
                    _columns[row1Element.Column].Swap(row1Element, null, row1, row2);
                    if (row1Element.Column == row2)
                        _diagonal[row1Element.Column] = row1Element;
                    row1Element = row1Element.NextInRow;
                }
                else if (row1Element == null)
                {
                    _columns[row2Element.Column].Swap(null, row2Element, row1, row2);
                    if (row2Element.Column == row1)
                        _diagonal[row2Element.Column] = row2Element;
                    row2Element = row2Element.NextInRow;
                }
                else if (row1Element.Column < row2Element.Column)
                {
                    _columns[row1Element.Column].Swap(row1Element, null, row1, row2);
                    if (row1Element.Column == row2)
                        _diagonal[row1Element.Column] = row1Element;
                    row1Element = row1Element.NextInRow;
                }
                else if (row2Element.Column < row1Element.Column)
                {
                    _columns[row2Element.Column].Swap(null, row2Element, row1, row2);
                    if (row2Element.Column == row1)
                        _diagonal[row2Element.Column] = row2Element;
                    row2Element = row2Element.NextInRow;
                }
                else
                {
                    _columns[row1Element.Column].Swap(row1Element, row2Element, row1, row2);

                    // Update diagonals
                    if (row1Element.Column == row2)
                        _diagonal[row1Element.Column] = row1Element;
                    else if (row2Element.Column == row1)
                        _diagonal[row2Element.Column] = row2Element;

                    row1Element = row1Element.NextInRow;
                    row2Element = row2Element.NextInRow;
                }
            }
        }

        /// <summary>
        /// Swap columns in the matrix
        /// </summary>
        /// <param name="column1">Column 1</param>
        /// <param name="column2">Column 2</param>
        public void SwapColumns(int column1, int column2)
        {
            if (column1 < 0 || column2 < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(column1, column2));
            if (column1 == column2)
                return;
            if (column2 < column1)
            {
                var tmp = column1;
                column1 = column2;
                column2 = tmp;
            }

            // Get the two elements
            SparseMatrixElement<T> column1Element = _columns[column1].FirstInColumn;
            SparseMatrixElement<T> column2Element = _columns[column2].FirstInColumn;

            // Swap the two rows
            var tmpColumn = _columns[column1];
            _columns[column1] = _columns[column2];
            _columns[column2] = tmpColumn;

            // Reset the diagonals
            _diagonal[column1] = null;
            _diagonal[column2] = null;

            // Swap the elements from left to right
            while (column1Element != null || column2Element != null)
            {
                if (column2Element == null)
                {
                    _rows[column1Element.Row].Swap(column1Element, null, column1, column2);
                    if (column1Element.Row == column2)
                        _diagonal[column1Element.Row] = column1Element;
                    column1Element = column1Element.NextInColumn;
                }
                else if (column1Element == null)
                {
                    _rows[column2Element.Row].Swap(null, column2Element, column1, column2);
                    if (column2Element.Row == column1)
                        _diagonal[column2Element.Row] = column2Element;
                    column2Element = column2Element.NextInColumn;
                }
                else if (column1Element.Row < column2Element.Row)
                {
                    _rows[column1Element.Row].Swap(column1Element, null, column1, column2);
                    if (column1Element.Row == column2)
                        _diagonal[column1Element.Row] = column1Element;
                    column1Element = column1Element.NextInColumn;
                }
                else if (column2Element.Row < column1Element.Row)
                {
                    _rows[column2Element.Row].Swap(null, column2Element, column1, column2);
                    if (column2Element.Row == column1)
                        _diagonal[column2Element.Row] = column2Element;
                    column2Element = column2Element.NextInColumn;
                }
                else
                {
                    _rows[column1Element.Row].Swap(column1Element, column2Element, column1, column2);

                    // Update diagonal
                    if (column1Element.Row == column2)
                        _diagonal[column1Element.Row] = column1Element;
                    if (column2Element.Row == column1)
                        _diagonal[column2Element.Row] = column2Element;

                    column1Element = column1Element.NextInColumn;
                    column2Element = column2Element.NextInColumn;
                }
            }
        }

        /// <summary>
        /// Expand matrix
        /// </summary>
        /// <param name="newSize">New supported matrix size</param>
        private void ExpandMatrix(int newSize)
        {
            // Current size
            Size = newSize;

            // No need to allocate new vectors
            if (newSize <= _allocatedSize)
                return;
            int oldAllocatedSize = _allocatedSize;

            // Allocate some extra space if necessary
            newSize = Math.Max(newSize, (int)(_allocatedSize * ExpansionFactor));

            // Resize rows
            Array.Resize(ref _rows, newSize + 1);
            for (int i = oldAllocatedSize + 1; i <= newSize; i++)
                _rows[i] = new Row<T>();

            // Resize columns
            Array.Resize(ref _columns, newSize + 1);
            for (int i = oldAllocatedSize + 1; i <= newSize; i++)
                _columns[i] = new Column<T>();

            // Other
            Array.Resize(ref _diagonal, newSize + 1);
            _allocatedSize = newSize;
        }
        
        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture.NumberFormat);
        }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="formatProvider">Format provider</param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            // Show the contents of the matrix
            string[][] displayData = new string[Size][];
            int[] columnWidths = new int[Size];
            for (int r = 1; r <= Size; r++)
            {
                // Initialize
                displayData[r - 1] = new string[Size];

                var element = _rows[r].FirstInRow;
                for (int c = 1; c <= Size; c++)
                {
                    // Go to the next element if necessary
                    if (element != null && element.Column < c)
                        element = element.NextInRow;

                    // Show the element
                    if (element == null || element.Column != c)
                        displayData[r - 1][c - 1] = "...";
                    else
                        displayData[r - 1][c - 1] = element.Value.ToString(format, formatProvider);
                    columnWidths[c - 1] = Math.Max(columnWidths[c - 1], displayData[r - 1][c - 1].Length);
                }
            }

            // Build the string
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    var displayElement = displayData[r][c];
                    sb.Append(new string(' ', columnWidths[c] - displayElement.Length + 2));
                    sb.Append(displayElement);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
