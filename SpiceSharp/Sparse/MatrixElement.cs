using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A matrix element
    /// </summary>
    public class MatrixElement
    {
        /// <summary>
        /// Real part
        /// </summary>
        public double Real;

        /// <summary>
        /// Imaginary part
        /// </summary>
        public double Imag;

        /// <summary>
        /// Row index
        /// </summary>
        internal int Row;

        /// <summary>
        /// Column index
        /// </summary>
        internal int Col;

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
        public static implicit operator double(MatrixElement el) => el.Real;

        /// <summary>
        /// Allow casting to a complex number
        /// </summary>
        /// <param name="el">Matrix element</param>
        public static implicit operator Complex(MatrixElement el) => new Complex(el.Real, el.Imag);
    }
}
