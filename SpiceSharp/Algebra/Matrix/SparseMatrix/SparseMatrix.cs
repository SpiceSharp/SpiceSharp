using System;
using System.Globalization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A square matrix that uses a sparse storage method with doubly-linked elements.
    /// </summary>
    /// <remarks>
    /// <para>The elements in row and column with index 0 are considered trashcan elements. They
    /// should all map on the same element.</para>
    /// <para>The matrix automatically expands size if necessary.</para>
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public partial class SparseMatrix<T> : IPermutableMatrix<T>, ISparseMatrix<T> where T : IFormattable
    {
        // TODO: Support removing ALL elements + do that at the end of the simulation.

        /// <summary>
        /// Constants
        /// </summary>
        private const int InitialSize = 4;
        private const float ExpansionFactor = 1.5f;

        /// <summary>
        /// Occurs when two rows are swapped.
        /// </summary>
        public event EventHandler<PermutationEventArgs> RowsSwapped;

        /// <summary>
        /// Occurs when two columns are swapped.
        /// </summary>
        public event EventHandler<PermutationEventArgs> ColumnsSwapped;

        /// <summary>
        /// Gets the number of elements in the matrix.
        /// </summary>
        /// <value>
        /// The element count.
        /// </value>
        public int ElementCount { get; private set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Gets or sets the value with the specified row.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value</returns>
        public T this[int row, int column]
        {
            get => GetMatrixValue(row, column);
            set => SetMatrixValue(row, column, value);
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private Row[] _rows;
        private Column[] _columns;
        private Element[] _diagonal;
        private readonly Element _trashCan;
        private int _allocatedSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix{T}"/> class.
        /// </summary>
        public SparseMatrix()
        {
            Size = 0;
            _allocatedSize = InitialSize;

            // Allocate rows
            _rows = new Row[InitialSize + 1];
            for (var i = 1; i <= InitialSize; i++)
                _rows[i] = new Row();

            // Allocate columns
            _columns = new Column[InitialSize + 1];
            for (var i = 1; i <= InitialSize; i++)
                _columns[i] = new Column();

            // Other
            _diagonal = new Element[InitialSize + 1];
            _trashCan = new Element(0, 0);
            ElementCount = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix{T}"/> class.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        public SparseMatrix(int size)
        {
            Size = size;
            _allocatedSize = Math.Max(InitialSize, size);

            // Allocate rows
            _rows = new Row[_allocatedSize + 1];
            for (var i = 1; i <= _allocatedSize; i++)
                _rows[i] = new Row();

            // Allocate columns
            _columns = new Column[_allocatedSize + 1];
            for (var i = 1; i <= _allocatedSize; i++)
                _columns[i] = new Column();

            // Other
            _diagonal = new Element[_allocatedSize + 1];
            _trashCan = new Element(0, 0);
            ElementCount = 1;
        }

        /// <summary>
        /// Gets a value in the matrix at a specific row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The value at the specified row and column.
        /// </returns>
        protected T GetMatrixValue(int row, int column)
        {
            var element = FindElement(row, column);
            if (element == null)
                return default;
            return element.Value;
        }

        /// <summary>
        /// Sets the value in the matrix at a specific row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        protected void SetMatrixValue(int row, int column, T value)
        {
            if (value.Equals(default))
            {
                // We don't need to create a new element unnecessarily
                var element = FindElement(row, column);
                if (element != null)
                    element.Value = default;
            }
            else
            {
                // We have to create an element if it doesn't exist yet
                var element = GetElement(row, column);
                element.Value = value;
            }
        }

        /// <summary>
        /// Get an element in the matrix. This method creates a new element if it doesn't exist.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The matrix element at the specified row and column.</returns>
        public Element<T> GetElement(int row, int column)
        {
            if (row < 0 || column < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));
            if (row == 0 || column == 0)
                return _trashCan;

            // Expand our matrix if it is necessary!
            if (row > Size || column > Size)
                Expand(Math.Max(row, column));

            // Quick access to diagonals
            if (row == column && _diagonal[row] != null)
                return _diagonal[row];

            if (!_rows[row].CreateGetElement(row, column, out var element))
            {
                ElementCount++;
                _columns[column].Insert(element);
                if (row == column)
                    _diagonal[row] = element;
            }

            return element;
        }

        /// <summary>
        /// Get a diagonal element.
        /// </summary>
        /// <param name="index">The row/column index.</param>
        /// <returns>The matrix element at the specified diagonal index.</returns>
        public Element<T> FindDiagonalElement(int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException(nameof(index));
            if (index > Size)
                return null;
            return _diagonal[index];
        }

        /// <summary>
        /// Finds the <see cref="ISparseMatrixElement{T}" /> on the diagonal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        ISparseMatrixElement<T> ISparseMatrix<T>.FindDiagonalElement(int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException(nameof(index));
            if (index > Size)
                return null;
            return _diagonal[index];
        }

        /// <summary>
        /// Find an element. This method will not create a new element if it doesn't exist.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The element at the specified row and column, or null if it doesn't exist.</returns>
        public Element<T> FindElement(int row, int column)
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
        /// Gets the first element in a row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The first element in the row or null if there are none.</returns>
        public ISparseMatrixElement<T> GetFirstInRow(int row) => _rows[row].FirstInRow;

        /// <summary>
        /// Gets the last element in a row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The last element in the row of null if there are none.</returns>
        public ISparseMatrixElement<T> GetLastInRow(int row) => _rows[row].LastInRow;

        /// <summary>
        /// Gets the first element in a column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The first element in the column or null if there are none.</returns>
        public ISparseMatrixElement<T> GetFirstInColumn(int column) => _columns[column].FirstInColumn;

        /// <summary>
        /// Gets the last element in a column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The last element in the column or null if there are none.</returns>
        public ISparseMatrixElement<T> GetLastInColumn(int column) => _columns[column].LastInColumn;

        /// <summary>
        /// Swaps two rows in the matrix.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        public void SwapRows(int row1, int row2)
        {
            if (row1 < 1 || row1 > Size)
                throw new ArgumentOutOfRangeException(nameof(row1));
            if (row2 < 1 || row2 > Size)
                throw new ArgumentOutOfRangeException(nameof(row2));
            if (row1 == row2)
                return;
            if (row2 < row1)
            {
                var tmp = row1;
                row1 = row2;
                row2 = tmp;
            }

            // Get the two elements
            var row1Element = _rows[row1].FirstInRow;
            var row2Element = _rows[row2].FirstInRow;

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
                    row1Element = row1Element.Right;
                }
                else if (row1Element == null)
                {
                    _columns[row2Element.Column].Swap(null, row2Element, row1, row2);
                    if (row2Element.Column == row1)
                        _diagonal[row2Element.Column] = row2Element;
                    row2Element = row2Element.Right;
                }
                else if (row1Element.Column < row2Element.Column)
                {
                    _columns[row1Element.Column].Swap(row1Element, null, row1, row2);
                    if (row1Element.Column == row2)
                        _diagonal[row1Element.Column] = row1Element;
                    row1Element = row1Element.Right;
                }
                else if (row2Element.Column < row1Element.Column)
                {
                    _columns[row2Element.Column].Swap(null, row2Element, row1, row2);
                    if (row2Element.Column == row1)
                        _diagonal[row2Element.Column] = row2Element;
                    row2Element = row2Element.Right;
                }
                else
                {
                    _columns[row1Element.Column].Swap(row1Element, row2Element, row1, row2);

                    // Update diagonals
                    if (row1Element.Column == row2)
                        _diagonal[row1Element.Column] = row1Element;
                    else if (row2Element.Column == row1)
                        _diagonal[row2Element.Column] = row2Element;

                    row1Element = row1Element.Right;
                    row2Element = row2Element.Right;
                }
            }

            OnRowsSwapped(new PermutationEventArgs(row1, row2));
        }

        /// <summary>
        /// Swaps two columns in the matrix.
        /// </summary>
        /// <param name="column1">The first column index.</param>
        /// <param name="column2">The second column index.</param>
        public void SwapColumns(int column1, int column2)
        {
            if (column1 < 1 || column1 > Size)
                throw new ArgumentOutOfRangeException(nameof(column1));
            if (column2 < 1 || column2 > Size)
                throw new ArgumentOutOfRangeException(nameof(column2));
            if (column1 == column2)
                return;
            if (column2 < column1)
            {
                var tmp = column1;
                column1 = column2;
                column2 = tmp;
            }

            // Get the two elements
            var column1Element = _columns[column1].FirstInColumn;
            var column2Element = _columns[column2].FirstInColumn;

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
                    column1Element = column1Element.Below;
                }
                else if (column1Element == null)
                {
                    _rows[column2Element.Row].Swap(null, column2Element, column1, column2);
                    if (column2Element.Row == column1)
                        _diagonal[column2Element.Row] = column2Element;
                    column2Element = column2Element.Below;
                }
                else if (column1Element.Row < column2Element.Row)
                {
                    _rows[column1Element.Row].Swap(column1Element, null, column1, column2);
                    if (column1Element.Row == column2)
                        _diagonal[column1Element.Row] = column1Element;
                    column1Element = column1Element.Below;
                }
                else if (column2Element.Row < column1Element.Row)
                {
                    _rows[column2Element.Row].Swap(null, column2Element, column1, column2);
                    if (column2Element.Row == column1)
                        _diagonal[column2Element.Row] = column2Element;
                    column2Element = column2Element.Below;
                }
                else
                {
                    _rows[column1Element.Row].Swap(column1Element, column2Element, column1, column2);

                    // Update diagonal
                    if (column1Element.Row == column2)
                        _diagonal[column1Element.Row] = column1Element;
                    if (column2Element.Row == column1)
                        _diagonal[column2Element.Row] = column2Element;

                    column1Element = column1Element.Below;
                    column2Element = column2Element.Below;
                }
            }

            OnColumnsSwapped(new PermutationEventArgs(column1, column2));
        }

        /// <summary>
        /// Resets all elements in the matrix to their default value.
        /// </summary>
        public void Reset()
        {
            _trashCan.Value = default;
            for (var r = 1; r <= Size; r++)
            {
                var elt = GetFirstInRow(r);
                while (elt != null)
                {
                    elt.Value = default;
                    elt = elt.Right;
                }
            }
        }

        /// <summary>
        /// Clears the matrix of any elements. The size of the matrix becomes 0.
        /// </summary>
        public void Clear()
        {
            _trashCan.Value = default;
            for (var i = 1; i < _columns.Length; i++)
                _columns[i].Clear();
            for (var i = 1; i < _rows.Length; i++)
                _rows[i].Clear();
            for (var i = 0; i < _diagonal.Length; i++)
                _diagonal[i] = null;
            Array.Resize(ref _columns, InitialSize + 1);
            Array.Resize(ref _rows, InitialSize + 1);
            Array.Resize(ref _diagonal, InitialSize + 1);
            _allocatedSize = InitialSize;
            Size = 0;
            ElementCount = 1;
        }

        /// <summary>
        /// Expands the matrix.
        /// </summary>
        /// <param name="newSize">The new matrix size.</param>
        private void Expand(int newSize)
        {
            // Only expanding here!
            if (newSize <= Size)
                return;

            // Current size
            Size = newSize;

            // No need to allocate new vectors
            if (newSize <= _allocatedSize)
                return;
            var oldAllocatedSize = _allocatedSize;

            // Allocate some extra space if necessary
            newSize = Math.Max(newSize, (int)(_allocatedSize * ExpansionFactor));

            // Resize rows
            Array.Resize(ref _rows, newSize + 1);
            for (var i = oldAllocatedSize + 1; i <= newSize; i++)
                _rows[i] = new Row();

            // Resize columns
            Array.Resize(ref _columns, newSize + 1);
            for (var i = oldAllocatedSize + 1; i <= newSize; i++)
                _columns[i] = new Column();

            // Other
            Array.Resize(ref _diagonal, newSize + 1);
            _allocatedSize = newSize;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture.NumberFormat);
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
            // Show the contents of the matrix
            var displayData = new string[Size][];
            var columnWidths = new int[Size];
            for (var r = 1; r <= Size; r++)
            {
                // Initialize
                displayData[r - 1] = new string[Size];

                var element = _rows[r].FirstInRow;
                for (var c = 1; c <= Size; c++)
                {
                    // Go to the next element if necessary
                    if (element != null && element.Column < c)
                        element = element.Right;

                    // Show the element
                    if (element == null || element.Column != c)
                        displayData[r - 1][c - 1] = "...";
                    else
                        displayData[r - 1][c - 1] = element.Value.ToString(format, formatProvider);
                    columnWidths[c - 1] = Math.Max(columnWidths[c - 1], displayData[r - 1][c - 1].Length);
                }
            }

            // Build the string
            var sb = new StringBuilder();
            for (var r = 0; r < Size; r++)
            {
                for (var c = 0; c < Size; c++)
                {
                    var displayElement = displayData[r][c];
                    sb.Append(new string(' ', columnWidths[c] - displayElement.Length + 2));
                    sb.Append(displayElement);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Raises the <see cref="RowsSwapped" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PermutationEventArgs"/> instance containing the event data.</param>
        protected virtual void OnRowsSwapped(PermutationEventArgs args) => RowsSwapped?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="ColumnsSwapped" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PermutationEventArgs"/> instance containing the event data.</param>
        protected virtual void OnColumnsSwapped(PermutationEventArgs args) => ColumnsSwapped?.Invoke(this, args);
    }
}
