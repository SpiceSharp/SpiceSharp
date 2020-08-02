using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Interface describing a history of a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IEnumerable{T}" />
    public interface IHistory<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets or sets the current value in the history.
        /// </summary>
        /// <value>
        /// The current value.
        /// </value>
        T Value { get; set; }

        /// <summary>
        /// Gets the previous value. An index of 0 means the current value.
        /// </summary>
        /// <param name="index">The number of points to go back.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="index"/> is out of range.</exception>
        T GetPreviousValue(int index);

        /// <summary>
        /// Gets the number of points tracked by the history.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        int Length { get; }

        /// <summary>
        /// Accepts the current point and moves on to the next.
        /// </summary>
        void Accept();

        /// <summary>
        /// Sets all values in the history to the same value.
        /// </summary>
        /// <param name="value">The value.</param>
        void Set(T value);

        /// <summary>
        /// Sets all values in the history to the value returned by the specified method.
        /// </summary>
        /// <param name="method">The method for creating the value. The index indicates the current point.</param>
        void Set(Func<int, T> method);
    }
}
