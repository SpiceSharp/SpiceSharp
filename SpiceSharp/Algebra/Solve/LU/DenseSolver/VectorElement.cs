namespace SpiceSharp.Algebra
{
    public abstract partial class DenseLUSolver<M, V, T>
    {
        /// <summary>
        /// An <see cref="ISolverElement{T}"/> that is returned by a <see cref="DenseLUSolver{M, V, T}"/>.
        /// </summary>
        protected abstract class VectorElement : ISolverElement<T>
        {
            /// <summary>
            /// Gets the index.
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index => _parent.Row[_index];

            private DenseLUSolver<M, V, T> _parent;
            private int _index;

            /// <summary>
            /// Initializes a new instance of the <see cref="VectorElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="index">The index.</param>
            public VectorElement(DenseLUSolver<M, V, T> parent, int index)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _index = index;
            }

            /// <summary>
            /// Adds the specified value to the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public abstract void Add(T value);

            /// <summary>
            /// Subtracts the specified value from the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public abstract void Subtract(T value);

            /// <summary>
            /// Sets the specified value for the matrix element.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(T value)
            {
                var r = _parent.Row[_index];
                _parent.Vector[r] = value;
            }

            /// <summary>
            /// Gets the value of the matrix element.
            /// </summary>
            /// <returns>
            /// The matrix element value.
            /// </returns>
            public T GetValue()
            {
                var r = _parent.Row[_index];
                return _parent.Vector[r];
            }
        }
    }
}
