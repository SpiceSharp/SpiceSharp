using System.Numerics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations.Base
{
    /// <summary>
    /// Complex solution vector
    /// </summary>
    public class ComplexSolution : Vector<Complex>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length"></param>
        public ComplexSolution(int length)
            : base(length)
        {
        }
    }
}
