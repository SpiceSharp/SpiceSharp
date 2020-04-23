using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A set of matrix and right-hand-side vector elements
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class ElementSet<T>
    {
        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        private readonly Element<T>[] _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="matrixPins">The Y-matrix indices.</param>
        /// <param name="rhsPins">The right-hand side vector indices.</param>
        public ElementSet(ISparseSolver<T> solver, MatrixLocation[] matrixPins, int[] rhsPins = null)
        {
            int length = (rhsPins?.Length ?? 0) + (matrixPins?.Length ?? 0);
            _elements = new Element<T>[length];
            int offset = 0;
            if (matrixPins != null)
            {
                for (var i = 0; i < matrixPins.Length; i++)
                    _elements[i] = solver.GetElement(matrixPins[i]);
                offset = matrixPins.Length;
            }
            if (rhsPins != null)
            {
                for (var i = 0; i < rhsPins.Length; i++)
                    _elements[i + offset] = solver.GetElement(rhsPins[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="matrixPins">The matrix pins.</param>
        public ElementSet(ISparseSolver<T> solver, params MatrixLocation[] matrixPins)
            : this(solver, matrixPins, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="rhsPins">The RHS pins.</param>
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
            for (var i = 0; i < values.Length; i++)
                _elements[i].Add(values[i]);
        }

        /// <summary>
        /// Subtracts the specified values. First come
        /// the matrix elements, then the RHS vector elements.
        /// </summary>
        /// <param name="values">The values.</param>
        public void Subtract(params T[] values)
        {
            for (var i = 0; i < values.Length; i++)
                _elements[i].Subtract(values[i]);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_elements.Length < 256)
            {
                var sb = new StringBuilder(_elements.Length * 10);
                sb.Append('(');
                bool isFirst = true;
                foreach (var elt in _elements)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(", ");
                    sb.Append(elt.ToString());
                }
                sb.Append(')');
                return sb.ToString();
            }
            else
                return "ElementSet ({0} elements)".FormatString(_elements.Length);
        }
    }
}
