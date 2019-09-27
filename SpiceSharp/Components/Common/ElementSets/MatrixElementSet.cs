using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A set of matrix elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public abstract class MatrixElementSet<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        protected IMatrixElement<T>[] Elements { get; }

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
        /// Initializes a new instance of the <see cref="MatrixElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public MatrixElementSet(ISolver<T> solver, params MatrixPin[] pins)
        {
            Solver = solver.ThrowIfNull(nameof(solver));
            Elements = new IMatrixElement<T>[pins.Length];
            for (var i = 0; i < pins.Length; i++)
                Elements[i] = solver.GetMatrixElement(pins[i].Row, pins[i].Column);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="pins">The pins.</param>
        public MatrixElementSet(ISolver<T> solver, List<MatrixPin> pins)
            : this(solver, pins.ToArray())
        {
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public virtual void Destroy()
        {
            for (var i = 0; i < Elements.Length; i++)
                Elements[i] = null;
        }
    }
}
