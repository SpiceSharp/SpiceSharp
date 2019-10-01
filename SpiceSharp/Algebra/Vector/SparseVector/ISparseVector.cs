using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a vector that can be stepped through.
    /// </summary>
    public interface ISparseVector<T> : IVector<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the first <see cref="ISparseVectorElement{T}"/> in the vector.
        /// </summary>
        /// <returns>The vector element.</returns>
        ISparseVectorElement<T> GetFirstInVector();

        /// <summary>
        /// Gets the last <see cref="ISparseVectorElement{T}"/> in the vector.
        /// </summary>
        /// <returns></returns>
        ISparseVectorElement<T> GetLastInVector();

        /// <summary>
        /// Gets a vector element at the specified index. If
        /// it doesn't exist, a new one is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        Element<T> GetVectorElement(int index);

        /// <summary>
        /// Finds a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element; otherwise <c>null</c>.
        /// </returns>
        Element<T> FindVectorElement(int index);
    }
}
