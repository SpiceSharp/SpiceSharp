using System;
using System.Text;
using SpiceSharp.NewSparse.Matrix;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Matrix using a sparse representation
    /// The matrix is always kept square!
    /// </summary>
    /// <typeparam name="T">Type for the element</typeparam>
    public class Matrix<T> where T : IFormattable
    {
        /// <summary>
        /// Constants
        /// </summary>
        const int InitialSize = 4;
        const float ExpansionFactor = 1.5f;

        /// <summary>
        /// Gets the matrix size
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        Row<T>[] rows;
        Column<T>[] columns;
        MatrixElement<T>[] diagonal;
        MatrixElement<T> trashCan;
        int allocatedSize;

        /// <summary>
        /// Finds an element in the matrix
        /// It does not create one if it doesn't exist. Use <see cref="GetElement(int, int)"/> instead to create elements.
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public T this[int row, int column]
        {
            get
            {
                var element = FindElement(row, column);
                if (element == null)
                    return default;
                return element.Value;
            }
            set
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
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Matrix()
        {
            Size = 1;
            allocatedSize = InitialSize;

            // Allocate rows
            rows = new Row<T>[InitialSize + 1];
            for (int i = 1; i <= InitialSize; i++)
                rows[i] = new Row<T>();

            // Allocate columns
            columns = new Column<T>[InitialSize + 1];
            for (int i = 1; i <= InitialSize; i++)
                columns[i] = new Column<T>();

            // Other
            diagonal = new MatrixElement<T>[InitialSize + 1];
            trashCan = new MatrixElement<T>(0, 0);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        public Matrix(int size)
        {
            if (size < 0)
                throw new ArgumentException("Invalid size {0}".FormatString(size));
            Size = size;
            allocatedSize = Math.Max(InitialSize, size);

            // Allocate rows
            rows = new Row<T>[allocatedSize + 1];
            for (int i = 1; i <= allocatedSize; i++)
                rows[i] = new Row<T>();

            // Allocate columns
            columns = new Column<T>[allocatedSize + 1];
            for (int i = 1; i <= allocatedSize; i++)
                columns[i] = new Column<T>();

            // Other
            diagonal = new MatrixElement<T>[allocatedSize + 1];
            trashCan = new MatrixElement<T>(0, 0);
        }

        /// <summary>
        /// Get an element
        /// Creates a new element if it doesn't exist
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public Element<T> GetElement(int row, int column)
        {
            if (row < 0 || column < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));
            if (row == 0 || column == 0)
                return trashCan;

            // Expand our matrix if it is necessary!
            if (row > Size || column > Size)
                ExpandMatrix(Math.Max(row, column));

            // Quick access to diagonals
            if (row == column && diagonal[row] != null)
                return diagonal[row];

            MatrixElement<T> element;
            if (!rows[row].CreateGetElement(row, column, out element))
            {
                columns[column].Insert(element);
                if (row == column)
                    diagonal[row] = element;
            }

            return element;
        }

        /// <summary>
        /// Get a diagonal element
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public Element<T> GetDiagonalElement(int index) => diagonal[index];
        
        /// <summary>
        /// Find an element
        /// Will not create a new element if it doesn't exist
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public Element<T> FindElement(int row, int column)
        {
            if (row < 0 || column < 0)
                throw new ArgumentException("Invalid indices ({0}, {1})".FormatString(row, column));
            if (row > Size || column > Size)
                return default;
            if (row == 0 || column == 0)
                return trashCan;

            // Find the element
            return rows[row].Find(column);
        }

        /// <summary>
        /// Get the first element in a row
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public Element<T> GetFirstInRow(int row) => rows[row].FirstInRow;

        /// <summary>
        /// Get the first element in a column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public Element<T> GetFirstInColumn(int column) => columns[column].FirstInColumn;

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

            // Get the two elements
            MatrixElement<T> row1Element = rows[row1].FirstInRow;
            MatrixElement<T> row2Element = rows[row2].FirstInRow;

            // Swap the two rows
            var tmpRow = rows[row1];
            rows[row1] = rows[row2];
            rows[row2] = tmpRow;

            // Swap the elements from left to right
            while (row1Element != null || row2Element != null)
            {
                if (row2Element == null)
                {
                    columns[row1Element.Column].Swap(row1Element, null, row1, row2);
                    if (row1Element.Column == row2)
                        diagonal[row1Element.Column] = row1Element;
                    row1Element = row1Element.NextInRow;
                }
                else if (row1Element == null)
                {
                    columns[row2Element.Column].Swap(null, row2Element, row1, row2);
                    if (row2Element.Column == row1)
                        diagonal[row2Element.Column] = row2Element;
                    row2Element = row2Element.NextInRow;
                }
                else if (row1Element.Column < row2Element.Column)
                {
                    columns[row1Element.Column].Swap(row1Element, null, row1, row2);
                    if (row1Element.Column == row2)
                        diagonal[row1Element.Column] = row1Element;
                    row1Element = row1Element.NextInRow;
                }
                else if (row2Element.Column < row1Element.Column)
                {
                    columns[row2Element.Column].Swap(null, row2Element, row1, row2);
                    if (row2Element.Column == row1)
                        diagonal[row2Element.Column] = row2Element;
                    row2Element = row2Element.NextInRow;
                }
                else
                {
                    columns[row1Element.Column].Swap(row1Element, row2Element, row1, row2);

                    // Update diagonals
                    if (row1Element.Column == row2)
                        diagonal[row1Element.Column] = row1Element;
                    else if (row2Element.Column == row1)
                        diagonal[row2Element.Column] = row2Element;

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

            // Get the two elements
            MatrixElement<T> column1Element = columns[column1].FirstInColumn;
            MatrixElement<T> column2Element = columns[column2].FirstInColumn;

            // Swap the two rows
            var tmpColumn = columns[column1];
            columns[column1] = columns[column2];
            columns[column2] = tmpColumn;

            // Swap the elements from left to right
            while (column1Element != null || column2Element != null)
            {
                if (column2Element == null)
                {
                    rows[column1Element.Row].Swap(column1Element, null, column1, column2);
                    if (column1Element.Row == column2)
                        diagonal[column1Element.Row] = column1Element;
                    column1Element = column1Element.NextInColumn;
                }
                else if (column1Element == null)
                {
                    rows[column2Element.Row].Swap(null, column2Element, column1, column2);
                    if (column2Element.Row == column1)
                        diagonal[column2Element.Row] = column2Element;
                    column2Element = column2Element.NextInColumn;
                }
                else if (column1Element.Row < column2Element.Row)
                {
                    rows[column1Element.Row].Swap(column1Element, null, column1, column2);
                    if (column1Element.Row == column2)
                        diagonal[column1Element.Row] = column1Element;
                    column1Element = column1Element.NextInColumn;
                }
                else if (column2Element.Row < column1Element.Row)
                {
                    rows[column2Element.Row].Swap(null, column2Element, column1, column2);
                    if (column2Element.Row == column1)
                        diagonal[column2Element.Row] = column2Element;
                    column2Element = column2Element.NextInColumn;
                }
                else
                {
                    rows[column1Element.Row].Swap(column1Element, column2Element, column1, column2);

                    // Update diagonal
                    if (column1Element.Row == column2)
                        diagonal[column1Element.Row] = column1Element;
                    if (column2Element.Row == column1)
                        diagonal[column2Element.Row] = column2Element;

                    column1Element = column1Element.NextInColumn;
                    column2Element = column2Element.NextInColumn;
                }
            }
        }
        
        /// <summary>
        /// Expand matrix
        /// </summary>
        /// <param name="newSize">New supported matrix size</param>
        void ExpandMatrix(int newSize)
        {
            // Current size
            Size = newSize;

            // No need to allocate new vectors
            if (newSize <= allocatedSize)
                return;
            int oldAllocatedSize = allocatedSize;

            // Allocate some extra space if necessary
            newSize = Math.Max(newSize, (int)(allocatedSize * ExpansionFactor));

            // Resize rows
            Array.Resize(ref rows, newSize + 1);
            for (int i = oldAllocatedSize + 1; i <= newSize; i++)
                rows[i] = new Row<T>();

            // Resize columns
            Array.Resize(ref columns, newSize + 1);
            for (int i = oldAllocatedSize + 1; i <= newSize; i++)
                columns[i] = new Column<T>();

            // Other
            Array.Resize(ref diagonal, newSize + 1);
            allocatedSize = newSize;
        }
        
        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null, System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
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

                var element = rows[r].FirstInRow;
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
