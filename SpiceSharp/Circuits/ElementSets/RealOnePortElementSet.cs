using SpiceSharp.Algebra;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Element set for a one-port for real numbers.
    /// </summary>
    /// <seealso cref="SpiceSharp.Circuits.RealMatrixElementSet" />
    public class RealOnePortElementSet : RealMatrixElementSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealOnePortElementSet"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        public RealOnePortElementSet(ISolver<double> solver, int a, int b)
            : base(solver, new [] { new MatrixPin(a, a), new MatrixPin(a, b), new MatrixPin(b, a), new MatrixPin(b, b) })
        {
        }

        /// <summary>
        /// Adds the specified value for the one-port to all necessary matrix elements.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AddOnePort(double value)
        {
            Elements[0].Value += value;
            Elements[1].Value -= value;
            Elements[2].Value -= value;
            Elements[3].Value += value;
        }
    }
}
