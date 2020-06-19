using System;

namespace SpiceSharp.Algebra.Solve
{
    public abstract partial class DenseLUSolver<T>
    {
        /// <summary>
        /// A matrix that keeps everything synchronized for our solver.
        /// </summary>
        /// <seealso cref="PivotingSolver{M, V, T}" />
        /// <seealso cref="ISolver{T}" />
        protected class ReorderedMatrix : IMatrix<T>
        {
            private readonly DenseLUSolver<T> _parent;

            /// <inheritdoc/>
            public int Size => _parent.Matrix.Size;

            /// <inheritdoc/>
            public T this[int row, int column]
            {
                get => _parent.Matrix[row, column];
                set => _parent.Matrix[row, column] = value;

            }

            /// <inheritdoc/>
            public T this[MatrixLocation location]
            {
                get => _parent.Matrix[location];
                set => _parent.Matrix[location] = value;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedMatrix"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public ReorderedMatrix(DenseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <inheritdoc/>
            public void SwapRows(int row1, int row2) => _parent.SwapRows(row1, row2);

            /// <inheritdoc/>
            public void SwapColumns(int column1, int column2) => _parent.SwapColumns(column1, column2);

            /// <inheritdoc/>
            public void Reset() => _parent.Matrix.Reset();

            /// <inheritdoc/>
            public void Clear() => _parent.Matrix.Reset();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString() => _parent.Matrix.ToString();
        }
    }
}
