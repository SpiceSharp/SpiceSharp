using SpiceSharp.Algebra;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A matrix set for real numbers.
    /// </summary>
    /// <seealso cref="MatrixElementSet{T}" />
    public class RealMatrixElementSet : MatrixElementSet<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealMatrixElementSet"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public RealMatrixElementSet(ISolver<double> solver, params MatrixPin[] pins)
            : base(solver, pins)
        {
        }

        /// <summary>
        /// Adds the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        public void Add(params double[] values)
        {
            for (var i = 0; i < values.Length; i++)
                Elements[i].Value += values[i];
        }
    }
}
