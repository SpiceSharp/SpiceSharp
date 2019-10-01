using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Template for a matrix element or a right-hand side vector element of a solver.
    /// </summary>
    /// <remarks>
    /// Rather than implementing an indexer, we want to discourage notations like
    /// element[5] += 1.0, because the operation is not atomic. It is encouraged
    /// to use <see cref="Add(T)"/> and <see cref="Subtract(T)"/> instead.
    /// </remarks>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ISolverElement<T> where T : IFormattable
    {
        /// <summary>
        /// Adds the specified value to the matrix element.
        /// </summary>
        /// <param name="value">The value.</param>
        void Add(T value);

        /// <summary>
        /// Subtracts the specified value from the matrix element.
        /// </summary>
        /// <param name="value">The value.</param>
        void Subtract(T value);

        /// <summary>
        /// Sets the specified value for the matrix element.
        /// </summary>
        /// <param name="value">The value.</param>
        void SetValue(T value);

        /// <summary>
        /// Gets the value of the matrix element.
        /// </summary>
        /// <returns>
        /// The matrix element value.
        /// </returns>
        T GetValue();
    }
}
