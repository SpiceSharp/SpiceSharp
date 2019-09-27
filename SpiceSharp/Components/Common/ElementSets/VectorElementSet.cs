using SpiceSharp.Algebra;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A set of vector elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class VectorElementSet<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        protected IVectorElement<T>[] Elements { get; }

        /// <summary>
        /// Gets the solver.
        /// </summary>
        /// <remarks>
        /// Can be used for locking.
        /// </remarks>
        /// <value>
        /// The solver.
        /// </value>
        protected ISolver<T> Solver { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public VectorElementSet(ISolver<T> solver, params int[] pins)
        {
            Solver = solver.ThrowIfNull(nameof(solver));
            Elements = new IVectorElement<T>[pins.Length];
            for (var i = 0; i < pins.Length; i++)
                Elements[i] = solver.GetVectorElement(pins[i]);
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            for (var i = 0; i < Elements.Length; i++)
                Elements[i] = null;
        }
    }
}
