using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Histories
{
    /// <summary>
    /// A class that implements a history with an array.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IHistory{T}" />
    public class ArrayHistory<T> : IHistory<T>
    {
        private readonly T[] _history;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public T Value
        {
            get => _history[0];
            set => _history[0] = value;
        }

        /// <summary>
        /// Gets the number of points tracked by the history.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length => _history.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        public ArrayHistory(int length)
        {
            length = Math.Max(length, 1);
            _history = new T[length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="defaultValue">The default value.</param>
        public ArrayHistory(int length, T defaultValue)
        {
            length = Math.Max(length, 1);
            _history = new T[length];
            for (int i = 0; i < length; i++)
                _history[i] = defaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="generator">The function that generates the initial values.</param>
        public ArrayHistory(int length, Func<int, T> generator)
        {
            generator.ThrowIfNull(nameof(generator));
            length = Math.Max(length, 1);
            for (int i = 0; i < length; i++)
                _history[i] = generator(i);
        }

        /// <summary>
        /// Cycles through history.
        /// </summary>
        public void Accept()
        {
            var tmp = _history[Length - 1];
            for (int i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = tmp;
        }

        /// <summary>
        /// Gets the previous value. An index of 0 means the current value.
        /// </summary>
        /// <param name="index">The number of points to go back.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or higher than the number of history points.</exception>
        public T GetPreviousValue(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _history[index];
        }

        /// <summary>
        /// Sets all values in the history to the same value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(T value)
        {
            for (int i = 0; i < _history.Length; i++)
                _history[i] = value;
        }

        /// <summary>
        /// Sets all values in the history to the value returned by the specified method.
        /// </summary>
        /// <param name="method">The method for creating the value. The index indicates the current point.</param>
        public void Set(Func<int, T> method)
        {
            method.ThrowIfNull(nameof(method));
            for (int i = 0; i < _history.Length; i++)
                _history[i] = method(i);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _history.Length; i++)
                yield return _history[i];
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _history.GetEnumerator();
    }
}
