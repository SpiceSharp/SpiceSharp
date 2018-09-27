namespace SpiceSharp.Algebra
{
    public partial class SparseVector<T>
    {
        /// <summary>
        /// A vector element for <see cref="SparseVector{T}"/>
        /// </summary>
        /// <seealso cref="Algebra.Vector{T}" />
        /// <seealso cref="System.IFormattable" />
        protected class SparseVectorElement : VectorElement<T>
        {
            /// <summary>
            /// Gets or sets the index.
            /// </summary>
            public new int Index
            {
                get => base.Index;
                set => base.Index = value;
            }

            /// <summary>
            /// Gets or sets the next element in the vector.
            /// </summary>
            public SparseVectorElement NextInVector { get; set; }

            /// <summary>
            /// Gets or sets the previous element in the vector.
            /// </summary>
            public SparseVectorElement PreviousInVector { get; set; }

            /// <summary>
            /// Gets the next element.
            /// </summary>
            public override VectorElement<T> Below => NextInVector;

            /// <summary>
            /// Gets the previous element.
            /// </summary>
            public override VectorElement<T> Above => PreviousInVector;

            /// <summary>
            /// Initializes a new instance of the <see cref="SparseVectorElement"/> class.
            /// </summary>
            /// <param name="index">The index of the element.</param>
            public SparseVectorElement(int index)
                : base(index)
            {
            }
        }
    }
}
