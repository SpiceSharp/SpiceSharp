using SpiceSharp.Algebra;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A matrix element set for complex numbers.
    /// </summary>
    /// <seealso cref="MatrixElementSet{T}" />
    public class ComplexMatrixElementSet : MatrixElementSet<Complex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexMatrixElementSet"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public ComplexMatrixElementSet(ISolver<Complex> solver, params MatrixPin[] pins)
            : base(solver, pins)
        {
        }

        /// <summary>
        /// Adds the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        public void Add(params Complex[] values)
        {
            lock (Solver)
            {
                for (var i = 0; i < values.Length; i++)
                    Elements[i].Value += values[i];
            }
        }
    }
}
