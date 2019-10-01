using SpiceSharp.Algebra;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A vector set for real numbers.
    /// </summary>
    /// <seealso cref="VectorElementSet{T}" />
    public class RealVectorElementSet : VectorElementSet<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealVectorElementSet"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public RealVectorElementSet(ISolver<double> solver, params int[] pins)
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
                Elements[i].Add(values[i]);
        }
    }
}
