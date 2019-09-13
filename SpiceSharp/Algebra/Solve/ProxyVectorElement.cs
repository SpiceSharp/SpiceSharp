using System;

namespace SpiceSharp.Algebra
{
    public abstract partial class LinearSystem<M, V, T>
    {
        /// <summary>
        /// A vector element that can be returned by <see cref="LinearSystem{T}"/>.
        /// </summary>
        /// <seealso cref="IElementMatrix{T}" />
        /// <seealso cref="IElementVector{T}" />
        /// <seealso cref="IFormattable" />
        protected class ProxyVectorElement : IVectorElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the vector element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value
            {
                get => _parent[Index];
                set => _parent[Index] = value;
            }

            /// <summary>
            /// Gets the external index.
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index { get; }

            private LinearSystem<M, V, T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyVectorElement"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="index">The index.</param>
            public ProxyVectorElement(LinearSystem<M, V, T> parent, int index)
            {
                _parent = parent;
                Index = index;
            }
        }
    }
}
