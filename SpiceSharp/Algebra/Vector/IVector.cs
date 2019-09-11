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
        /// Gets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        T GetVectorValue(int index);

        /// <summary>
        /// Sets the value of the vector at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        void SetVectorValue(int index, T value);

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
