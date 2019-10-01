using System;

namespace SpiceSharp.Algebra
{
    public partial class DenseRealSolver<M, V> where M : IPermutableMatrix<double>
        where V : IPermutableVector<double>
    {
        /// <summary>
        /// An <see cref="ISolverElement{T}"/> for vector elements in a <see cref="DenseRealSolver{M, V}"/>.
        /// </summary>
        /// <seealso cref="DenseLUSolver{M, V, T}"/>
        protected class RealSolverVectorElement : ISolverElement<double>
        {
            private DenseRealSolver<M, V> _parent;
            private int _row;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="row"></param>
            public RealSolverVectorElement(DenseRealSolver<M, V> parent, int row)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _row = row;
            }

            /// <summary>
            /// Adds the specified value to the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Add(double value)
            {
                int r = _parent.Row[_row];
                _parent.Vector[r] += value;
            }

            /// <summary>
            /// Subtracts the specified value from the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Subtract(double value)
            {
                int r = _parent.Row[_row];
                _parent.Vector[r] -= value;
            }

            /// <summary>
            /// Sets the specified value for the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(double value)
            {
                int r = _parent.Row[_row];
                _parent.Vector[r] = value;
            }

            /// <summary>
            /// Gets the value of the matrix element.
            /// </summary>
            /// <returns>
            /// The matrix element value.
            /// </returns>
            public double GetValue()
            {
                int r = _parent.Row[_row];
                return _parent.Vector[r];
            }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="String" /> that represents this instance.
            /// </returns>
            public override string ToString()
                => GetValue().ToString();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="provider">The provider.</param>
            /// <returns>
            /// A <see cref="String" /> that represents this instance.
            /// </returns>
            public string ToString(string format, IFormatProvider provider)
                => GetValue().ToString(format, provider);
        }
    }
}
