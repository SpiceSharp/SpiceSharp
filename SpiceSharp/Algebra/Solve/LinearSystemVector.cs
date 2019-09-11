namespace SpiceSharp.Algebra
{
    public abstract partial class LinearSystem<T>
    {
        /// <summary>
        /// This class can be used by <see cref="LinearSystem{T}"/> for preconditioning
        /// the Y-matrix. If the preconditioner decides to swap rows/columns, this class
        /// will make sure that the translation is tracked.
        /// </summary>
        /// <seealso cref="IPermutableVector{T}"/>
        protected class LinearSystemVector : IPermutableVector<T>
        {
            private LinearSystem<T> _parent;

            /// <summary>
            /// Gets or sets the value at the specified index.
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
            /// Gets the length of the vector.
            /// </summary>
            /// <value>
            /// The length.
            /// </value>
            public int Length => _parent.Vector.Length;

            /// <summary>
            /// Initializes a new instance of the <see cref="LinearSystemVector"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public LinearSystemVector(LinearSystem<T> parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Copies the contents of the vector to another one.
            /// </summary>
            /// <param name="target">The target vector.</param>
            public void CopyTo(IVector<T> target) => _parent.Vector.CopyTo(target);

            /// <summary>
            /// Finds a vector element at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            /// The vector element; otherwise <c>null</c>.
            /// </returns>
            public IVectorElement<T> FindVectorElement(int index) => _parent.Vector.FindVectorElement(index);

            /// <summary>
            /// Gets the first <see cref="IVectorElement{T}"/> in the vector.
            /// </summary>
            /// <returns>The vector element.</returns>
            public IVectorElement<T> GetFirstInVector() => _parent.Vector.GetFirstInVector();

            /// <summary>
            /// Gets a vector element at the specified index. If
            /// it doesn't exist, a new one is created.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>
            /// The vector element.
            /// </returns>
            public IVectorElement<T> GetVectorElement(int index) => _parent.Vector.GetVectorElement(index);

            /// <summary>
            /// Gets the value of the vector at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>The value.</returns>
            public T GetVectorValue(int index) => _parent.Vector.GetVectorValue(index);

            /// <summary>
            /// Sets the value of the vector at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <param name="value">The value.</param>
            public void SetVectorValue(int index, T value) => _parent.Vector.SetVectorValue(index, value);

            /// <summary>
            /// Swaps two elements in the vector.
            /// </summary>
            /// <param name="index1">The first index.</param>
            /// <param name="index2">The second index.</param>
            public void SwapElements(int index1, int index2) => _parent.SwapRows(index1, index2);

            /// <summary>
            /// Resets all elements in the vector.
            /// </summary>
            public void ResetVector() => _parent.Vector.ResetVector();
        }
    }
}
