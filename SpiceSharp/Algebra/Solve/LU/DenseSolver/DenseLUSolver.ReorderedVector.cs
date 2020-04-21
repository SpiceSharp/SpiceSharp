using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Algebra.Solve
{
    public abstract partial class DenseLUSolver<T>
    {
        /// <summary>
        /// A vector that keeps everything synchronized for our solver.
        /// </summary>
        /// <seealso cref="PivotingSolver{M, V, T}" />
        /// <seealso cref="ISolver{T}" />
        protected class ReorderedVector : IVector<T>
        {
            private readonly DenseLUSolver<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedVector"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public ReorderedVector(DenseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Gets the length of the vector.
            /// </summary>
            /// <value>
            /// The length.
            /// </value>
            public int Length => _parent.Vector.Length;

            /// <summary>
            /// Gets or sets the value at the specified index.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="index">The index.</param>
            /// <returns></returns>
            public T this[int index] 
            {
                get => _parent.Vector[index];
                set => _parent.Vector[index] = value;
            }

            /// <summary>
            /// Swaps two elements in the vector.
            /// </summary>
            /// <param name="index1">The first index.</param>
            /// <param name="index2">The second index.</param>
            public void SwapElements(int index1, int index2)
            {
                // This is why we had to implement our own reordered matrix...
                _parent.SwapRows(index1, index2);
            }

            /// <summary>
            /// Copies the contents of the vector to another one.
            /// </summary>
            /// <param name="target">The target vector.</param>
            public void CopyTo(IVector<T> target) => _parent.Vector.CopyTo(target);

            /// <summary>
            /// Resets all elements in the vector to their default value.
            /// </summary>
            public void Reset() => _parent.Vector.Reset();

            /// <summary>
            /// Clears all elements in the vector. The size of the vector becomes 0.
            /// </summary>
            public void Clear() => _parent.Vector.Clear();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString() => _parent.Vector.ToString();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="formatProvider">The format provider.</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public string ToString(string format, IFormatProvider formatProvider) => _parent.Vector.ToString(format, formatProvider);
        }
    }
}
