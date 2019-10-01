using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A simple struct for describing a matrix row/column location.
    /// </summary>
    public struct MatrixLocation
    {
        /// <summary>
        /// The row index.
        /// </summary>
        public int Row;

        /// <summary>
        /// The column index.
        /// </summary>
        public int Column;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixLocation"/> struct.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public MatrixLocation(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "({0},{1})".FormatString(Row, Column);
        }
    }
}
