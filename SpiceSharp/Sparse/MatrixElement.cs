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
        public int Row = 0;

        /// <summary>
        /// The column index
        /// </summary>
        public int Col = 0;

        /// <summary>
        /// Next matrix element in the same row
        /// </summary>
        internal MatrixElement NextInRow = null;

        /// <summary>
        /// Next matrix element in the same column
        /// </summary>
        internal MatrixElement NextInCol = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        public MatrixElement(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override string ToString()
        {
            return $"({Row}, {Col}) = {Value}";
        }

        /// <summary>
        /// Overload addition
        /// </summary>
        /// <param name="value"></param>
        public void Add(double value) => Value.Real += value;

        /// <summary>
        /// Overload subtraction
        /// </summary>
        /// <param name="value"></param>
        public void Sub(double value) => Value.Real -= value;

        /// <summary>
        /// Overload addition
        /// </summary>
        /// <param name="value"></param>
        public void Add(Complex value) => Value.Cplx += value;

        /// <summary>
        /// Overload subtraction
        /// </summary>
        /// <param name="value"></param>
        public void Sub(Complex value) => Value.Cplx -= value;
        
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
