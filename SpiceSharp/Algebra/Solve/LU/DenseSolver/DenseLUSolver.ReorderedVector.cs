using System;

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

            /// <inheritdoc/>
            public int Length => _parent.Vector.Length;

            /// <inheritdoc/>
            public T this[int index]
            {
                get => _parent.Vector[index];
                set => _parent.Vector[index] = value;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedVector"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public ReorderedVector(DenseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <inheritdoc/>
            public void SwapElements(int index1, int index2)
            {
                // This is why we had to implement our own reordered matrix...
                _parent.SwapRows(index1, index2);
            }

            /// <inheritdoc/>
            public void CopyTo(IVector<T> target) => _parent.Vector.CopyTo(target);

            /// <inheritdoc/>
            public void Reset() => _parent.Vector.Reset();

            /// <inheritdoc/>
            public void Clear() => _parent.Vector.Clear();

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString() => _parent.Vector.ToString();
        }
    }
}
