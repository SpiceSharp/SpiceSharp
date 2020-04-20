using System;
using System.Collections.Generic;
using System.Text;

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

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedMatrix"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public ReorderedMatrix(DenseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Gets the size of the matrix.
            /// </summary>
            /// <value>
            /// The matrix size.
            /// </value>
            public int Size => _parent.Matrix.Size;

            /// <summary>
            /// Gets or sets the value with the specified row.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="row">The row.</param>
            /// <param name="column">The column.</param>
            /// <returns></returns>
            public T this[int row, int column]
            {
                get => _parent.Matrix[row, column];
                set => _parent.Matrix[row, column] = value;
            
            }

            /// <summary>
            /// Gets or sets the value with the specified location.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="location">The location.</param>
            /// <returns></returns>
            public T this[MatrixLocation location] 
            {
                get => _parent.Matrix[location]; 
                set => _parent.Matrix[location] = value;
            }

            /// <summary>
            /// Swaps two rows in the matrix.
            /// </summary>
            /// <param name="row1">The first row index.</param>
            /// <param name="row2">The second row index.</param>
            public void SwapRows(int row1, int row2) => _parent.SwapRows(row1, row2);

            /// <summary>
            /// Swaps two columns in the matrix.
            /// </summary>
            /// <param name="column1">The first column index.</param>
            /// <param name="column2">The second column index.</param>
            public void SwapColumns(int column1, int column2) => _parent.SwapColumns(column1, column2);

            /// <summary>
            /// Resets all elements in the matrix to their default value.
            /// </summary>
            public void Reset() => _parent.Matrix.Reset();

            /// <summary>
            /// Clears the matrix of any elements. The size of the matrix becomes 0.
            /// </summary>
            public void Clear() => _parent.Matrix.Reset();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString() => _parent.Matrix.ToString();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="formatProvider">The format provider.</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public string ToString(string format, IFormatProvider formatProvider) => _parent.Matrix.ToString(format, formatProvider);
        }
    }
}
