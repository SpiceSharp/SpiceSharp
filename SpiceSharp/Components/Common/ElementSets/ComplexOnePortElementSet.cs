using SpiceSharp.Algebra;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Element set for a one-port for complex numbers.
    /// </summary>
    /// <seealso cref="ComplexMatrixElementSet" />
    public class ComplexOnePortElementSet : ComplexMatrixElementSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealOnePortElementSet"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        public ComplexOnePortElementSet(ISolver<Complex> solver, int a, int b)
            : base(solver, new[] { new MatrixPin(a, a), new MatrixPin(a, b), new MatrixPin(b, a), new MatrixPin(b, b) })
        {
        }

        /// <summary>
        /// Adds the specified value for the one-port to all necessary matrix elements.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AddOnePort(Complex value)
        {
            Elements[0].Add(value);
            Elements[1].Subtract(value);
            Elements[2].Subtract(value);
            Elements[3].Add(value);
        }
    }
}
