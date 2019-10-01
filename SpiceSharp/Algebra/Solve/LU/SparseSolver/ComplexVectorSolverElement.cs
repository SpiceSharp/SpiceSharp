using System;
using System.Numerics;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    public partial class SparseComplexSolver<M, V> where M : IPermutableMatrix<Complex>, ISparseMatrix<Complex>
        where V : IPermutableVector<Complex>, ISparseVector<Complex>
    {
        /// <summary>
        /// An <see cref="ISolverElement{T}"/> for RHS elements in a <see cref="SparseLUSolver{M, V, T}"/>
        /// </summary>
        /// <seealso cref="SparseLUSolver{M, V, T}" />
        protected class ComplexVectorSolverElement : ISolverElement<Complex>
        {
            private Element<Complex> _element;

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexVectorSolverElement"/> class.
            /// </summary>
            /// <param name="element">The element.</param>
            public ComplexVectorSolverElement(Element<Complex> element)
            {
                _element = element.ThrowIfNull(nameof(element));
            }

            /// <summary>
            /// Adds the specified value.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Add(Complex value) => _element.Value += value;

            /// <summary>
            /// Subtracts the specified value.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Subtract(Complex value) => _element.Value -= value;

            /// <summary>
            /// Sets the specified value for the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(Complex value) => _element.Value = value;

            /// <summary>
            /// Gets the value of the matrix element.
            /// </summary>
            /// <returns>
            /// The matrix element value.
            /// </returns>
            public Complex GetValue() => _element.Value;

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="String" /> that represents this instance.
            /// </returns>
            public override string ToString()
                => _element.ToString();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="provider">The provider.</param>
            /// <returns>
            /// A <see cref="String" /> that represents this instance.
            /// </returns>
            public string ToString(string format, IFormatProvider provider)
                => _element.ToString(format, provider);
        }
    }
}
