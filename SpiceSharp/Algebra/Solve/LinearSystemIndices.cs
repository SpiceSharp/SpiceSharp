namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Indices of an element in a linear system.
    /// </summary>
    public class LinearSystemIndices
    {
        /// <summary>
        /// Gets or sets the row in the matrix.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Gets or sets the column in the matrix.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="LinearSystemIndices"/> class.
        /// </summary>
        public LinearSystemIndices()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LinearSystemIndices"/> class.
        /// </summary>
        /// <param name="diagonal">The diagonal index.</param>
        public LinearSystemIndices(int diagonal)
        {
            Row = diagonal;
            Column = diagonal;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LinearSystemIndices"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public LinearSystemIndices(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
