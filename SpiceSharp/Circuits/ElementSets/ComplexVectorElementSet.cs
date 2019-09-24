using SpiceSharp.Algebra;
using System.Numerics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Vector elements for complex numbers.
    /// </summary>
    /// <seealso cref="VectorElementSet{T}" />
    public class ComplexVectorElementSet : VectorElementSet<Complex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVectorElementSet"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public ComplexVectorElementSet(ISolver<Complex> solver, params int[] pins)
            : base(solver, pins)
        {
        }

        /// <summary>
        /// Adds the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        public void Add(params Complex[] values)
        {
            for (var i = 0; i < values.Length; i++)
                Elements[i].Value += values[i];
        }
    }
}
