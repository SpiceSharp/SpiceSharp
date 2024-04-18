using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A set of matrix and right-hand-side vector elements
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class ElementSet<T>
    {
        private readonly Element<T>[] _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="matrixPins">The Y-matrix indices.</param>
        /// <param name="rhsPins">The right-hand side vector indices.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solver"/> is <c>null</c>.</exception>
        public ElementSet(ISparseSolver<T> solver, MatrixLocation[] matrixPins, int[] rhsPins = null)
        {
            solver.ThrowIfNull(nameof(solver));

            // Allocate memory for all the elements
            int length = (rhsPins?.Length ?? 0) + (matrixPins?.Length ?? 0);
            _elements = new Element<T>[length];
            int offset = 0;

            if (matrixPins != null)
            {
                for (int i = 0; i < matrixPins.Length; i++)
                    _elements[i] = solver.GetElement(matrixPins[i]);
                offset = matrixPins.Length;
            }

            if (rhsPins != null)
            {
                for (int i = 0; i < rhsPins.Length; i++)
                    _elements[i + offset] = solver.GetElement(rhsPins[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="matrixPins">The matrix pins.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solver"/> is <c>null</c>.</exception>
        public ElementSet(ISparseSolver<T> solver, params MatrixLocation[] matrixPins)
            : this(solver, matrixPins, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="rhsPins">The RHS pins.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solver"/> is <c>null</c>.</exception>
        public ElementSet(ISparseSolver<T> solver, params int[] rhsPins)
            : this(solver, null, rhsPins)
        {
        }

        /// <summary>
        /// Adds the specified values to each element. First come
        /// the matrix elements, then the RHS vector elements.
        /// </summary>
        /// <param name="values">The values.</param>
        public void Add(params T[] values)
        {
            if (values == null)
                return;
            for (int i = 0; i < values.Length; i++)
                _elements[i].Add(values[i]);
        }

        /// <summary>
        /// Subtracts the specified values. First come
        /// the matrix elements, then the RHS vector elements.
        /// </summary>
        /// <param name="values">The values.</param>
        public void Subtract(params T[] values)
        {
            if (values == null)
                return;
            for (int i = 0; i < values.Length; i++)
                _elements[i].Subtract(values[i]);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => "ElementSet ({0})".FormatString(_elements.Length);
    }
}
