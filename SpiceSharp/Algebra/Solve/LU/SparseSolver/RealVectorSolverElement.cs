using System;

namespace SpiceSharp.Algebra
{
    public partial class SparseRealSolver<M, V> where M : IPermutableMatrix<double>, ISparseMatrix<double>
        where V : IPermutableVector<double>, ISparseVector<double>
    {
        /// <summary>
        /// An <see cref="ISolverElement{T}"/> for a <see cref="SparseRealSolver{M, V}"/>.
        /// </summary>
        /// <seealso cref="SparseLUSolver{M, V, T}" />
        protected class RealVectorSolverElement : ISolverElement<double>
        {
            private Element<double> _element;

            /// <summary>
            /// Initializes a new instance of the <see cref="RealVectorSolverElement"/> class.
            /// </summary>
            /// <param name="element">The element.</param>
            public RealVectorSolverElement(Element<double> element)
            {
                _element = element.ThrowIfNull(nameof(element));
            }

            /// <summary>
            /// Adds the specified value to the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Add(double value) => _element.Value += value;

            /// <summary>
            /// Subtracts the specified value from the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Subtract(double value) => _element.Value -= value;

            /// <summary>
            /// Sets the specified value for the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(double value) => _element.Value = value;

            /// <summary>
            /// Gets the value of the matrix element.
            /// </summary>
            /// <returns>
            /// The matrix element value.
            /// </returns>
            public double GetValue() => _element.Value;

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
