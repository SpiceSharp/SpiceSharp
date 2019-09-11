namespace SpiceSharp.Algebra
{
    public partial class DenseVector<T>
    {
        /// <summary>
        /// Trash can element for a <see cref="DenseVector{T}"/>.
        /// </summary>
        /// <seealso cref="SpiceSharp.Algebra.IPermutableVector{T}" />
        /// <seealso cref="System.IFormattable" />
        public class TrashCanElement : IVectorElement<T>
        {
            /// <summary>
            /// Gets or sets the value of the vector element.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value { get; set; }

            /// <summary>
            /// Gets the index.
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index => 0;

            /// <summary>
            /// Gets the vector element above this one.
            /// </summary>
            /// <value>
            /// The vector element.
            /// </value>
            public IVectorElement<T> Above => null;

            /// <summary>
            /// Gets the vector element below this one.
            /// </summary>
            /// <value>
            /// The vector element.
            /// </value>
            public IVectorElement<T> Below => null;
        }
    }
}
