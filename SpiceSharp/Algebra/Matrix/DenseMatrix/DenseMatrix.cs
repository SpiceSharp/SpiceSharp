using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A square matrix using a dense representation.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public partial class DenseMatrix<T> : IPermutableMatrix<T> where T : IFormattable
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const int InitialSize = 4;
        private const float ExpansionFactor = 1.25f; // 1.25^2 approx. 1.55

        /// <summary>
        /// Occurs when two rows are swapped.
        /// </summary>
        public event EventHandler<PermutationEventArgs> RowsSwapped;

        /// <summary>
        /// Occurs when two columns are swapped.
        /// </summary>
        public event EventHandler<PermutationEventArgs> ColumnsSwapped;

        /// <summary>
        /// Gets the size of the matrix.
        /// </summary>
        /// <value>
        /// The matrix size.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private T[] _array;
        private T _trashCan;
        private int _allocatedSize;

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
        /// Initializes a new instance of the <see cref="DenseMatrix{T}"/> class.
        /// </summary>
        public DenseMatrix()
        {
            Size = 0;
            _allocatedSize = InitialSize;
            _array = new T[_allocatedSize * _allocatedSize];
            _trashCan = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix{T}"/> class.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        public DenseMatrix(int size)
        {
            Size = size;
            _allocatedSize = Math.Max(InitialSize, size);
            _array = new T[_allocatedSize * _allocatedSize];
            _trashCan = default;
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
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row == 0 || column == 0)
                return _trashCan;
            if (row > Size || column > Size)
                return default;
            row--;
            column--;
            return _array[row * _allocatedSize + column];
        }

        /// <summary>
        /// Sets the value in the matrix at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        public void SetMatrixValue(int row, int column, T value)
        {
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row == 0 || column == 0)
                _trashCan = value;
            else
            {
                if (!EqualityComparer<T>.Default.Equals(value, default) && (row > Size || column > Size))
                    Expand(Math.Max(row, column));
                row--;
                column--;
                _array[row * _allocatedSize + column] = value;
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
            var displayData = new string[Size][];
            var columnWidths = new int[Size];
            for (var r = 1; r <= Size; r++)
            {
                // Initialize
                displayData[r - 1] = new string[Size];
                for (var c = 1; c <= Size; c++)
                {
                    displayData[r - 1][c - 1] = _array[(r - 1) * _allocatedSize + c - 1].ToString(format, formatProvider);
                    columnWidths[c - 1] = Math.Max(columnWidths[c - 1], displayData[r - 1][c - 1].Length);
                }
            }

            // Build the string
            var sb = new StringBuilder();
            for (var r = 0; r < Size; r++)
            {
                for (var c = 0; c < Size; c++)
                {
                    var displayElt = displayData[r][c];
                    sb.Append(new string(' ', columnWidths[c] - displayElt.Length + 2));
                    sb.Append(displayElt);
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Swaps two rows in the matrix.
        /// </summary>
        /// <param name="row1">The first row index.</param>
        /// <param name="row2">The second row index.</param>
        public void SwapRows(int row1, int row2)
        {
            if (row1 <= 0 || row1 > Size)
                throw new ArgumentOutOfRangeException(nameof(row1));
            if (row2 <= 0 || row2 > Size)
                throw new ArgumentOutOfRangeException(nameof(row2));
            if (row1 == row2)
                return;

            var offset1 = (row1 - 1) * _allocatedSize;
            var offset2 = (row2 - 1) * _allocatedSize;
            for (var i = 0; i < Size; i++)
            {
                var tmp = _array[offset1 + i];
                _array[offset1 + i] = _array[offset2 + i];
                _array[offset2 + i] = tmp;
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

            column1--;
            column2--;
            for (var i = 0; i < _allocatedSize * _allocatedSize; i += _allocatedSize)
            {
                var tmp = _array[column1 + i];
                _array[column1 + i] = _array[column2 + i];
                _array[column2 + i] = tmp;
            }

            OnColumnsSwapped(new PermutationEventArgs(column1, column2));
        }

        /// <summary>
        /// Resets all elements in the matrix to their default value.
        /// </summary>
        public void Reset()
        {
            for (var i = 0; i < _array.Length; i++)
                _array[i] = default;
        }

        /// <summary>
        /// Clears the matrix of any elements. The size of the matrix becomes 0.
        /// </summary>
        public void Clear()
        {
            _trashCan = default;
            _array = new T[InitialSize * InitialSize];
            _allocatedSize = InitialSize;
            Size = 0;
        }

        /// <summary>
        /// Expands the matrix.
        /// </summary>
        /// <param name="newSize">The new matrix size.</param>
        private void Expand(int newSize)
        {
            var oldSize = Size;
            Size = newSize;
            if (newSize <= _allocatedSize)
                return;
            newSize = Math.Max(newSize, (int)(_allocatedSize * ExpansionFactor));

            // Create a new array and copy its contents
            var nArray = new T[newSize * newSize];
            for (var r = 0; r < oldSize; r++)
                for (var c = 0; c < oldSize; c++)
                    nArray[r * newSize + c] = _array[r * _allocatedSize + c];
            _array = nArray;
            _allocatedSize = newSize;
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
