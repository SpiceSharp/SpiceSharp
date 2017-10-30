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
        public ElementValue Value;

        /// <summary>
        /// The row index
        /// </summary>
        public int Row;

        /// <summary>
        /// The column index
        /// </summary>
        public int Col;
        
        /// <summary>
        /// Next matrix element in the same row
        /// </summary>
        internal MatrixElement NextInRow;

        /// <summary>
        /// Next matrix element in the same column
        /// </summary>
        internal MatrixElement NextInCol;

        /// <summary>
        /// Allow casting to a double
        /// </summary>
        /// <param name="el">Matrix element</param>
        public static implicit operator double(MatrixElement el) => el.Value.Real;

        /// <summary>
        /// Allow casting to a complex number
        /// </summary>
        /// <param name="el">Matrix element</param>
        public static implicit operator Complex(MatrixElement el) => el.Value.Cplx;

        /// <summary>
        /// Allow casting to an value
        /// </summary>
        /// <param name="el"></param>
        public static implicit operator ElementValue(MatrixElement el) => el.Value;
    }
}
