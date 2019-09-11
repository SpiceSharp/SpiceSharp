namespace SpiceSharp.Algebra
{
    public partial class DenseVector<T>
    {
        /// <summary>
        /// A vector element for a <see cref="DenseVector{T}"/>
        /// </summary>
        public class Element : IVectorElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the vector element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value
            {
                get => _parent.GetVectorValue(Index);
                set => _parent.SetVectorValue(Index, value);
            }

            /// <summary>
            /// Gets the index.
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index { get; }

            /// <summary>
            /// Gets the vector element above this one.
            /// </summary>
            /// <value>
            /// The vector element.
            /// </value>
            public IVectorElement<T> Above
            {
                get
                {
                    if (Index > 0)
                        return new Element(_parent, Index - 1);
                    return null;
                }
            }

            /// <summary>
            /// Gets the vector element below this one.
            /// </summary>
            /// <value>
            /// The vector element.
            /// </value>
            public IVectorElement<T> Below
            {
                get
                {
                    if (Index < _parent.Length)
                        return new Element(_parent, Index + 1);
                    return null;
                }
            }

            private DenseVector<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="Element" /> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="index">The index.</param>
            public Element(DenseVector<T> parent, int index)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                Index = index;
            }
        }
    }
}
