namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector element for an <see cref="ISparseVector{T}"/>. This element has links
    /// to the next and previous elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ISparseVectorElement<T>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        T Value { get; set; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        int Index { get; }

        /// <summary>
        /// Gets the non-default <see cref="ISparseVectorElement{T}"/> above this one.
        /// </summary>
        /// <value>
        /// The vector element.
        /// </value>
        ISparseVectorElement<T> Above { get; }

        /// <summary>
        /// Gets the non-default <see cref="ISparseVectorElement{T}"/> below this one.
        /// </summary>
        /// <value>
        /// The vector element.
        /// </value>
        ISparseVectorElement<T> Below { get; }
    }
}
