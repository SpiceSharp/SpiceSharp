using System;

namespace SpiceSharp.Algebra.Solve
{
    public abstract partial class SparseLUSolver<T>
    {
        /// <summary>
        /// A sparse vector that keeps both the matrix and right hand side vector synchronized for our solver.
        /// </summary>
        /// <seealso cref="SparseLUSolver{T}" />
        protected class ReorderedVector : ISparseVector<T>
        {
            private readonly SparseLUSolver<T> _parent;

            /// <inheritdoc/>
            public int ElementCount => _parent.Vector.ElementCount;

            /// <inheritdoc/>
            public int Length => _parent.Vector.Length;

            /// <inheritdoc/>
            public T this[int index]
            {
                get => _parent.Vector[index];
                set => _parent.Vector[index] = value;
            }

            /// <inheritdoc/>
            public void SwapElements(int index1, int index2)
            {
                // This is why we had to implement our own reordered matrix...
                _parent.SwapRows(index1, index2);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedVector"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public ReorderedVector(SparseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <inheritdoc/>
            public ISparseVectorElement<T> GetFirstInVector() => _parent.Vector.GetFirstInVector();

            /// <inheritdoc/>
            public ISparseVectorElement<T> GetLastInVector() => _parent.Vector.GetLastInVector();

            /// <inheritdoc/>
            public Element<T> GetElement(int index) => _parent.Vector.GetElement(index);

            /// <inheritdoc/>
            public bool RemoveElement(int index) => _parent.Vector.RemoveElement(index);

            /// <inheritdoc/>
            public Element<T> FindElement(int index) => _parent.Vector.FindElement(index);

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
