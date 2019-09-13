using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Describes a vector that can use elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IElementVector<T> : IVector<T> where T : IFormattable
    {
        /// <summary>
        /// Gets a vector element at the specified index. If
        /// it doesn't exist, a new one is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        IVectorElement<T> GetVectorElement(int index);

        /// <summary>
        /// Finds a vector element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The vector element; otherwise <c>null</c>.
        /// </returns>
        IVectorElement<T> FindVectorElement(int index);
    }
}
