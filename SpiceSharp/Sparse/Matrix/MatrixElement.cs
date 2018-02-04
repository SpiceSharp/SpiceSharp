namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A matrix element
    /// </summary>
    public class MatrixElement<T>
    {
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public Element<T> Element { get; }

        /// <summary>
        /// The row index
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// The column index
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Next matrix element in the same row
        /// </summary>
        internal MatrixElement<T> NextInRow { get; set; }

        /// <summary>
        /// Next matrix element in the same column
        /// </summary>
        internal MatrixElement<T> NextInColumn { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        public MatrixElement(int row, int column)
        {
            Element = ElementFactory.Create<T>();
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "({0}, {1}) = {2}".FormatString(Row, Column, Element);
        }

        /// <summary>
        /// Allow casting to an value
        /// </summary>
        /// <param name="el"></param>
        public static implicit operator Element<T>(MatrixElement<T> el)
        {
            if (el == null)
                return null;
            return el.Element;
        }
    }
}
