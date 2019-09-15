using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A matrix element for an <see cref="ISparseMatrix{T}"/>. This element has links
    /// to the surrounding matrix elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="IMatrixElement{T}" />
    public interface ISparseMatrixElement<T> : IMatrixElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> left of this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Left { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> right of this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Right { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> above this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Above { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> below this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Below { get; }
    }
}
