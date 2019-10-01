using SpiceSharp.Algebra;
using System;
using System.Text;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A set of matrix and right-hand-side vector elements
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class ElementSet<T> : IFormattable where T : IFormattable
    {
        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        private readonly ISolverElement<T>[] _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="matrixPins">The Y-matrix indices.</param>
        /// <param name="rhsPins">The right-hand side vector indices.</param>
        public ElementSet(ISolver<T> solver, MatrixLocation[] matrixPins, int[] rhsPins = null)
        {
            int length = (rhsPins?.Length ?? 0) + (matrixPins?.Length ?? 0);
            _elements = new ISolverElement<T>[length];
            int offset = 0;
            if (matrixPins != null)
            {
                for (var i = 0; i < matrixPins.Length; i++)
                    _elements[i] = solver.GetElement(matrixPins[i].Row, matrixPins[i].Column);
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
        public ElementSet(ISolver<T> solver, params MatrixLocation[] matrixPins)
            : this(solver, matrixPins, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementSet{T}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="rhsPins">The RHS pins.</param>
        public ElementSet(ISolver<T> solver, params int[] rhsPins)
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
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            for (var i = 0; i < _elements.Length; i++)
                _elements[i] = null;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
            => ToString(null, System.Globalization.CultureInfo.CurrentCulture);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider provider)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < _elements.Length; i++)
                sb.Append(_elements[i].ToString(format, provider));
            sb.Append("]");
            return sb.ToString();
        }
    }
}
