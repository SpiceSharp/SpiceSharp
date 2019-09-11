namespace SpiceSharp.Algebra
{
    public partial class SparseVector<T>
    {
        /// <summary>
        /// A vector element for <see cref="SparseVector{T}"/>
        /// </summary>
        protected class Element : IVectorElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the vector element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value { get; set; }

            /// <summary>
            /// Gets or sets the index.
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the next element in the vector.
            /// </summary>
            public Element NextInVector { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the vector.
            /// </summary>
            public Element PreviousInVector { get; set; }

            /// <summary>
            /// Gets the next element.
            /// </summary>
            IVectorElement<T> IVectorElement<T>.Below => NextInVector;

            /// <summary>
            /// Gets the previous element.
            /// </summary>
            IVectorElement<T> IVectorElement<T>.Above => PreviousInVector;

            /// <summary>
            /// Initializes a new instance of the <see cref="Element"/> class.
            /// </summary>
            /// <param name="index">The index.</param>
            public Element(int index)
            {
                Index = index;
            }
        }
    }
}
