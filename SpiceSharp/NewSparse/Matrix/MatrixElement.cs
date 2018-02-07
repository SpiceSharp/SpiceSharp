namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Element in a matrix
    /// Used by <see cref="Matrix{T}"/> to store values
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public class MatrixElement<T>
    {
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public Element<T> Element { get; }

        /// <summary>
        /// Gets or sets the row index
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Gets or sets the column index
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Next element in the row
        /// </summary>
        public MatrixElement<T> NextInRow;

        /// <summary>
        /// Next element in the column
        /// </summary>
        public MatrixElement<T> NextInColumn;

        /// <summary>
        /// Previous element in the row
        /// </summary>
        public MatrixElement<T> PreviousInRow;

        /// <summary>
        /// Previous element in the column
        /// </summary>
        public MatrixElement<T> PreviousInColumn;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        public MatrixElement(int row, int column)
        {
            Element = ElementFactory.Create<T>();
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "({0}, {1}) = {2}".FormatString(Row, Column, Element);
        }
    }
}
