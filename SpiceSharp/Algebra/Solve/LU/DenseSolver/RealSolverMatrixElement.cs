using System;

namespace SpiceSharp.Algebra
{
    public partial class DenseRealSolver<M, V> where M : IPermutableMatrix<double>
        where V : IPermutableVector<double>
    {
        /// <summary>
        /// An <see cref="ISolverElement{T}"/> for matrix elements in a <see cref="DenseRealSolver{M, V}"/>.
        /// </summary>
        /// <seealso cref="DenseLUSolver{M, V, T}" />
        protected class RealSolverMatrixElement : ISolverElement<double>
        {
            private DenseRealSolver<M, V> _parent;
            private int _row, _column;

            /// <summary>
            /// Initializes a new instance of the <see cref="RealSolverMatrixElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="row">The row.</param>
            /// <param name="column">The column.</param>
            public RealSolverMatrixElement(DenseRealSolver<M, V> parent, int row, int column)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _row = row;
                _column = column;
            }

            /// <summary>
            /// Adds the specified value to the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Add(double value)
            {
                var r = _parent.Row[_row];
                var c = _parent.Column[_column];
                _parent.Matrix[r, c] += value;
            }

            /// <summary>
            /// Subtracts the specified value from the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void Subtract(double value)
            {
                var r = _parent.Row[_row];
                var c = _parent.Column[_column];
                _parent.Matrix[r, c] -= value;
            }

            /// <summary>
            /// Sets the specified value for the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(double value)
            {
                var r = _parent.Row[_row];
                var c = _parent.Column[_column];
                _parent.Matrix[r, c] = value;
            }

            /// <summary>
            /// Gets the value of the matrix element.
            /// </summary>
            /// <returns>
            /// The matrix element value.
            /// </returns>
            public double GetValue()
            {
                var r = _parent.Row[_row];
                var c = _parent.Column[_column];
                return _parent.Matrix[r, c];
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
            public string ToString(string format, IFormatProvider provider)
                => GetValue().ToString(format, provider);
        }
    }
}
