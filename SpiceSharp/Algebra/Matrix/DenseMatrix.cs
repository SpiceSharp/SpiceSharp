using System;
using System.Globalization;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A square matrix using a dense representation. As a small optimization, the row and column with
    /// index 0 are not considered trash can elements.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class DenseMatrix<T> : Matrix<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly T[] _array;

        /// <summary>
        /// Gets or sets a value at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns></returns>
        public T this[int row, int column]
        {
            get
            {
                if (row < 0 || column < 0 || row >= Size || column >= Size)
                    throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));

                // Make indices 0-referenced
                return _array[row * Size + column];
            }
            set
            {
                if (row < 0 || column < 0 || row >= Size || column >= Size)
                    throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));

                // Make indices 0-referenced
                _array[row * Size + column] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix{T}"/> class.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        public DenseMatrix(int size)
            : base(size)
        {
            _array = new T[size * size];
        }

        /// <summary>
        /// Gets a value in the matrix at a specific row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The value at the specified row and column.
        /// </returns>
        public override T GetValue(int row, int column)
        {
            if (row < 0 || column < 0 || row >= Size || column >= Size)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));

            // Make indices 0-referenced
            return _array[row * Size + column];
        }

        /// <summary>
        /// Sets the value in the matrix at a specific row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(int row, int column, T value)
        {
            if (row < 0 || column < 0 || row >= Size || column >= Size)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));
            _array[row * Size + column] = value;
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
    }
}
