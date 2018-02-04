using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Real solution vector
    /// </summary>
    public class RealSolution : Vector<double>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Vector length</param>
        public RealSolution(int length)
            : base(length)
        {
        }
    }
}
