using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A template for tracking a history of objects.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IEnumerable{T}" />
    public abstract class History<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public abstract T Current { get; set; }

        /// <summary>
        /// Gets a value in history.
        /// </summary>
        /// <param name="index">The number of points to go back. 0 means the current point.</param>
        /// <returns>
        /// The object in history.
        /// </returns>
        public abstract T this[int index] { get; }

        /// <summary>
        /// Gets the number of timesteps stored.
        /// </summary>
        public int Length { get; protected set; }

        /// <summary>
        /// Gets all points in the history.
        /// </summary>
        protected abstract IEnumerable<T> Points { get; }

        /// <summary>
        /// Cycles through history.
        /// </summary>
        public abstract void Cycle();

        /// <summary>
        /// Store a new value in the history.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public abstract void Store(T newValue);

        /// <summary>
        /// Expand the history length.
        /// </summary>
        /// <param name="newLength">The new number of points in history to track.</param>
        public abstract void Expand(int newLength);

        /// <summary>
        /// Clear the whole history with the same value.
        /// </summary>
        /// <param name="value">The value to be cleared with.</param>
        public abstract void Clear(T value);

        /// <summary>
        /// Clear the history using a function by index.
        /// </summary>
        /// <param name="generator">The function generating the values.</param>
        public abstract void Clear(Func<int, T> generator);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => Points.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => Points.GetEnumerator();
    }
}
