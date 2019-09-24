using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// A structure keeping track of row and column indices.
    /// </summary>
    public struct MatrixPin
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
        /// Initializes a new instance of the <see cref="MatrixPin"/> struct.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public MatrixPin(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
