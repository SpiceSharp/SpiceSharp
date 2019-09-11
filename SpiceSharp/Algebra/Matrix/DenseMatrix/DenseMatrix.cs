using System;
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
        // TODO: Need to update created elements for permutations.

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
        private readonly T[] _array;
        private TrashCanElement _trashCan;

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
        /// <param name="size">The matrix size.</param>
        public DenseMatrix(int size)
        {
            Size = size;
            _array = new T[size * size];
            _trashCan = new TrashCanElement(this);
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
            if (row < 0 || row > Size)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column > Size)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row == 0 || column == 0)
                return _trashCan.Value;
            row--;
            column--;
            return _array[row * Size + column];
        }

        /// <summary>
        /// Sets the value in the matrix at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        public void SetMatrixValue(int row, int column, T value)
        {
            if (row < 0 || row > Size)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column > Size)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row == 0 || column == 0)
                _trashCan.Value = value;
            else
            {
                row--;
                column--;
                _array[row * Size + column] = value;
            }
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
        public IMatrixElement<T> GetMatrixElement(int row, int column)
        {
            if (row < 0 || row > Size)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column > Size)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row == 0 || column == 0)
                return _trashCan;
            return new Element(this, row, column);
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
            => GetMatrixElement(row, column);

        /// <summary>
        /// Gets the diagonal element at the specified row/column.
        /// </summary>
        /// <param name="index">The row/column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public IMatrixElement<T> FindDiagonalElement(int index)
            => GetMatrixElement(index, index);

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
                    displayData[r - 1][c - 1] = _array[(r - 1) * Size + c - 1].ToString(format, formatProvider);
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

            var offset1 = (row1 - 1) * Size;
            var offset2 = (row2 - 1) * Size;
            for (var i = 1; i <= Size; i++)
            {
                var tmp = _array[offset1 + i];
                _array[offset1 + i] = _array[offset2 + i];
                _array[offset2 + i] = tmp;
            }
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

            for (var i = 0; i < Size * Size; i += Size)
            {
                var tmp = _array[column1 + i];
                _array[column1 + i] = _array[column2 + i];
                _array[column2 + i] = tmp;
            }
        }

        /// <summary>
        /// Gets the first <see cref="IMatrixElement{T}" /> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public IMatrixElement<T> GetFirstInRow(int row)
            => GetMatrixElement(row, 1);

        /// <summary>
        /// Gets the last <see cref="IMatrixElement{T}" /> in the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public IMatrixElement<T> GetLastInRow(int row)
            => GetMatrixElement(row, Size);

        /// <summary>
        /// Gets the first <see cref="IMatrixElement{T}" /> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public IMatrixElement<T> GetFirstInColumn(int column)
            => GetMatrixElement(1, column);

        /// <summary>
        /// Gets the last <see cref="IMatrixElement{T}" /> in the specified column.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public IMatrixElement<T> GetLastInColumn(int column)
            => GetMatrixElement(Size, column);

        /// <summary>
        /// Resets all elements in the matrix.
        /// </summary>
        public void ResetMatrix()
        {
            for (var i = 0; i < _array.Length; i++)
                _array[i] = default;
        }
    }
}
