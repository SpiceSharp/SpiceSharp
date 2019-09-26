using System;

namespace SpiceSharp.Algebra
{
    public abstract partial class DenseLUSolver<M, V, T>
    {
        /// <summary>
        /// A <see cref="IVectorElement{T}"/> that is returned by a <see cref="DenseLUSolver{M, V, T}"/>.
        /// </summary>
        /// <seealso cref="SpiceSharp.Algebra.LinearSystem{M, V, T}" />
        protected class VectorElement : IVectorElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the vector element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            /// <exception cref="NotImplementedException">
            /// </exception>
            public T Value
            {
                get => _parent.Vector[Index];
                set => _parent.Vector[Index] = value;
            }

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
        }
    }
}
