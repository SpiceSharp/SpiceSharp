using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A vector element for an <see cref="ISparseVector{T}"/>. This element has links
    /// to the next and previous elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="IVectorElement{T}" />
    public interface ISparseVectorElement<T> : IVectorElement<T> where T : IFormattable
    {
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
