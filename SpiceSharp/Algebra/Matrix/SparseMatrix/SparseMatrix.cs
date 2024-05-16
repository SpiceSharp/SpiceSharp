using System;

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
    public partial class SparseMatrix<T> : IMatrix<T>, ISparseMatrix<T>
    {
        private Row[] _rows;
        private Column[] _columns;
        private Element[] _diagonal;
        private readonly Element _trashCan;
        private int _allocatedSize;

        private const int _initialSize = 4;
        private const float _expansionFactor = 1.5f;

        /// <inheritdoc/>
        public int ElementCount { get; private set; }

        /// <inheritdoc/>
        public int Size { get; private set; }

        /// <inheritdoc/>
        public T this[int row, int column]
        {
            get => GetMatrixValue(new MatrixLocation(row, column));
            set => SetMatrixValue(new MatrixLocation(row, column), value);
        }

        /// <inheritdoc/>
        public T this[MatrixLocation location]
        {
            get => GetMatrixValue(location);
            set => SetMatrixValue(location, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix{T}"/> class.
        /// </summary>
        public SparseMatrix()
        {
            Size = 0;
            _allocatedSize = _initialSize;

            // Allocate rows
            _rows = new Row[_initialSize + 1];
            for (int i = 1; i <= _initialSize; i++)
                _rows[i] = new Row();

            // Allocate columns
            _columns = new Column[_initialSize + 1];
            for (int i = 1; i <= _initialSize; i++)
                _columns[i] = new Column();

            // Other
            _diagonal = new Element[_initialSize + 1];
            _trashCan = new Element(new MatrixLocation());
            ElementCount = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix{T}"/> class.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="size"/> is negative.</exception>
        public SparseMatrix(int size)
        {
            Size = size.GreaterThanOrEquals(nameof(size), 0);
            _allocatedSize = Math.Max(_initialSize, size);

            // Allocate rows
            _rows = new Row[_allocatedSize + 1];
            for (int i = 1; i <= _allocatedSize; i++)
                _rows[i] = new Row();

            // Allocate columns
            _columns = new Column[_allocatedSize + 1];
            for (int i = 1; i <= _allocatedSize; i++)
                _columns[i] = new Column();

            // Other
            _diagonal = new Element[_allocatedSize + 1];
            _trashCan = new Element(new MatrixLocation());
            ElementCount = 1;
        }

        /// <inheritdoc/>
        public Element<T> GetElement(MatrixLocation location)
        {
            if (location.Row == 0 || location.Column == 0)
                return _trashCan;

            // Expand our matrix if it is necessary!
            if (location.Row > Size || location.Column > Size)
                Expand(Math.Max(location.Row, location.Column));

            // Quick access to diagonals
            if (location.Row == location.Column && _diagonal[location.Row] != null)
                return _diagonal[location.Row];

            if (!_rows[location.Row].CreateOrGetElement(location, out var element))
            {
                ElementCount++;
                _columns[location.Column].Insert(element);
                if (location.Row == location.Column)
                    _diagonal[location.Row] = element;
            }

            return element;
        }

        /// <inheritdoc/>
        public bool RemoveElement(MatrixLocation location)
        {
            if (location.Row < 1 || location.Column < 1)
                return false;
            if (location.Row > Size || location.Column > Size)
                return false;

            // Quick access to diagonals
            if (location.Row == location.Column)
            {
                if (_diagonal[location.Row] != null)
                {
                    _rows[location.Row].Remove(_diagonal[location.Row]);
                    _columns[location.Column].Remove(_diagonal[location.Column]);
                    _diagonal[location.Row] = null;
                    return true;
                }
                return false;
            }

            // General case
            var elt = _rows[location.Row].Find(location.Column);
            if (elt == null)
                return false;
            _rows[location.Row].Remove(elt);
            _columns[location.Column].Remove(elt);
            return true;
        }

        /// <inheritdoc/>
        public ISparseMatrixElement<T> FindDiagonalElement(int index)
        {
            index.GreaterThanOrEquals(nameof(index), 0);
            if (index > Size)
                return null;
            return _diagonal[index];
        }

        /// <inheritdoc/>
        public Element<T> FindElement(MatrixLocation location)
        {
            if (location.Row > Size || location.Column > Size)
                return null;
            if (location.Row == 0 || location.Column == 0)
                return _trashCan;

            // Find the element
            return _rows[location.Row].Find(location.Column);
        }

        /// <inheritdoc/>
        public ISparseMatrixElement<T> GetFirstInRow(int row)
            => row.GreaterThanOrEquals(nameof(row), 0) > Size ? null : _rows[row].FirstInRow;

        /// <inheritdoc/>
        public ISparseMatrixElement<T> GetLastInRow(int row)
            => row.GreaterThanOrEquals(nameof(row), 0) > Size ? null : _rows[row].LastInRow;

        /// <inheritdoc/>
        public ISparseMatrixElement<T> GetFirstInColumn(int column)
            => column.GreaterThanOrEquals(nameof(column), 0) > Size ? null : _columns[column].FirstInColumn;

        /// <inheritdoc/>
        public ISparseMatrixElement<T> GetLastInColumn(int column)
            => column.GreaterThanOrEquals(nameof(column), 0) > Size ? null : _columns[column].LastInColumn;

        /// <inheritdoc/>
        public void SwapRows(int row1, int row2)
        {
            row1.GreaterThan(nameof(row1), 0);
            row2.GreaterThan(nameof(row2), 0);
            if (row1 == row2)
                return;

            // Simplify algorithm: first index is always the lowest one
            if (row2 < row1)
                (row2, row1) = (row1, row2);
            if (row2 > Size)
                Expand(row2);

            // Get the two elements
            var row1Element = _rows[row1].FirstInRow;
            var row2Element = _rows[row2].FirstInRow;

            // Swap the two rows
            (_rows[row2], _rows[row1]) = (_rows[row1], _rows[row2]);

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
        }

        /// <inheritdoc/>
        public void SwapColumns(int column1, int column2)
        {
            column1.GreaterThan(nameof(column1), 0);
            column2.GreaterThan(nameof(column2), 0);
            if (column1 == column2)
                return;

            // Simplify algorithm: column1 is always the lowest index
            if (column2 < column1)
            {
                (column2, column1) = (column1, column2);
            }
            if (column2 > Size)
                Expand(column2);

            // Get the two elements
            var column1Element = _columns[column1].FirstInColumn;
            var column2Element = _columns[column2].FirstInColumn;

            // Swap the two rows
            (_columns[column2], _columns[column1]) = (_columns[column1], _columns[column2]);

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
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _trashCan.Value = default;
            for (int r = 1; r <= Size; r++)
            {
                var elt = GetFirstInRow(r);
                while (elt != null)
                {
                    elt.Value = default;
                    elt = elt.Right;
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _trashCan.Value = default;
            for (int i = 1; i < _columns.Length; i++)
                _columns[i].Clear();
            for (int i = 1; i < _rows.Length; i++)
                _rows[i].Clear();
            for (int i = 0; i < _diagonal.Length; i++)
                _diagonal[i] = null;
            Array.Resize(ref _columns, _initialSize + 1);
            Array.Resize(ref _rows, _initialSize + 1);
            Array.Resize(ref _diagonal, _initialSize + 1);
            _allocatedSize = _initialSize;
            Size = 0;
            ElementCount = 1;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => "Sparse matrix ({0}x{0})".FormatString(Size);

        private T GetMatrixValue(MatrixLocation location)
        {
            var element = FindElement(location);
            if (element == null)
                return default;
            return element.Value;
        }
        private void SetMatrixValue(MatrixLocation location, T value)
        {
            if (value.Equals(default))
            {
                // We don't need to create a new element unnecessarily
                var element = FindElement(location);
                if (element != null)
                    element.Value = default;
            }
            else
            {
                // We have to create an element if it doesn't exist yet
                var element = GetElement(location);
                element.Value = value;
            }
        }
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
            int oldAllocatedSize = _allocatedSize;

            // Allocate some extra space if necessary
            newSize = Math.Max(newSize, (int)(_allocatedSize * _expansionFactor));

            // Resize rows
            Array.Resize(ref _rows, newSize + 1);
            for (int i = oldAllocatedSize + 1; i <= newSize; i++)
                _rows[i] = new Row();

            // Resize columns
            Array.Resize(ref _columns, newSize + 1);
            for (int i = oldAllocatedSize + 1; i <= newSize; i++)
                _columns[i] = new Column();

            // Other
            Array.Resize(ref _diagonal, newSize + 1);
            _allocatedSize = newSize;
        }
    }
}
