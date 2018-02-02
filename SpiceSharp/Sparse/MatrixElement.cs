using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A matrix element
    /// </summary>
    public class MatrixElement
    {
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public ElementValue Value { get; }

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
        internal MatrixElement NextInRow { get; set; }

        /// <summary>
        /// Next matrix element in the same column
        /// </summary>
        internal MatrixElement NextInColumn { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        public MatrixElement(int row, int column)
        {
            Value = new ElementValue();
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "({0}, {1}) = {2}".FormatString(Row, Column, Value);
        }

        /// <summary>
        /// Allow casting to an value
        /// </summary>
        /// <param name="el"></param>
        public static implicit operator ElementValue(MatrixElement el)
        {
            if (el == null)
                return new ElementValue();
            return el.Value;
        }
    }
}
