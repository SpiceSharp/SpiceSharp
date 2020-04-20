using System;

namespace SpiceSharp.Algebra.Solve
{
    public abstract partial class SparseLUSolver<T>
    {
        /// <summary>
        /// A sparse vector that keeps everything synchronized for our solver.
        /// </summary>
        /// <seealso cref="SparseLUSolver{T}" />
        protected class ReorderedVector : ISparseVector<T>
        {
            private readonly SparseLUSolver<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReorderedVector"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public ReorderedVector(SparseLUSolver<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Gets the number of elements in the vector.
            /// </summary>
            /// <value>
            /// The element count.
            /// </value>
            public int ElementCount => _parent.Vector.ElementCount;

            /// <summary>
            /// Gets the first <see cref="ISparseVectorElement{T}" /> in the vector.
            /// </summary>
            /// <returns>
            /// The vector element.
            /// </returns>
            public ISparseVectorElement<T> GetFirstInVector() => _parent.Vector.GetFirstInVector();

            /// <summary>
            /// Gets the last <see cref="ISparseVectorElement{T}" /> in the vector.
            /// </summary>
            /// <returns></returns>
            public ISparseVectorElement<T> GetLastInVector() => _parent.Vector.GetLastInVector();

            /// <summary>
            /// Gets a vector element at the specified index. If
            /// it doesn't exist, a new one is created.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            /// The vector element.
            /// </returns>
            public Element<T> GetElement(int index) => _parent.Vector.GetElement(index);

            /// <summary>
            /// Finds a vector element at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            /// The vector element; otherwise <c>null</c>.
            /// </returns>
            public Element<T> FindElement(int index) => _parent.Vector.FindElement(index);

            /// <summary>
            /// Gets the length of the vector.
            /// </summary>
            /// <value>
            /// The length.
            /// </value>
            public int Length => _parent.Vector.Length;

            /// <summary>
            /// Gets or sets the value of the right hand side vector at the specified index.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <param name="index">The index.</param>
            /// <returns>The value.</returns>
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
