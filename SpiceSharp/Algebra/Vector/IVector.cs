using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A description of a vector.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IVector<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        int Length { get; }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        T this[int index] { get; set; }

        /// <summary>
        /// Copies the contents of the vector to another one.
        /// </summary>
        /// <param name="target">The target vector.</param>
        void CopyTo(IVector<T> target);

        /// <summary>
        /// Resets all elements in the vector.
        /// </summary>
        void ResetVector();
    }
}
