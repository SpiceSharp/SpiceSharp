using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a vector that can be stepped through.
    /// </summary>
    public interface ISparseVector<T> : IElementVector<T> where T : IFormattable
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
    }
}
