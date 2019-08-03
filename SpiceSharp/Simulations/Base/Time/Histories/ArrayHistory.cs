using System;
using System.Collections.Generic;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A class that implements a history with an array.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="History{T}" />
    public class ArrayHistory<T> : History<T>
    {
        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public override T Current
        {
            get => _history[0];
            set => _history[0] = value;
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value at the specified index.
        /// </returns>
        public override T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                return _history[index];
            }
        }

        /// <summary>
        /// Gets all points in the history.
        /// </summary>
        protected override IEnumerable<T> Points => _history;

        /// <summary>
        /// Timesteps in history
        /// </summary>
        private T[] _history;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        public ArrayHistory(int length)
        {
            Length = length;
            _history = new T[length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="defaultValue">The default value.</param>
        public ArrayHistory(int length, T defaultValue)
        {
            Length = length;
            _history = new T[length];
            for (var i = 0; i < length; i++)
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

            Length = length;
            for (var i = 0; i < length; i++)
                _history[i] = generator(i);
        }

        /// <summary>
        /// Cycles through history.
        /// </summary>
        public override void Cycle()
        {
            var tmp = _history[Length - 1];
            for (var i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = tmp;
        }

        /// <summary>
        /// Store a new value in the history.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void Store(T newValue)
        {
            // Shift the history
            for (var i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = newValue;
        }

        /// <summary>
        /// Expands the specified new size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        public override void Expand(int newSize)
        {
            if (newSize < Length)
                return;
            Array.Resize(ref _history, newSize);
            Length = newSize;
        }

        /// <summary>
        /// Clear the whole history with the same value.
        /// </summary>
        /// <param name="value">The value to be cleared with.</param>
        public override void Clear(T value)
        {
            for (var i = 0; i < Length; i++)
                _history[i] = value;
        }

        /// <summary>
        /// Clear the history using a function by index.
        /// </summary>
        /// <param name="generator">The function generating the values.</param>
        public override void Clear(Func<int, T> generator)
        {
            generator.ThrowIfNull(nameof(generator));

            for (var i = 0; i < Length; i++)
                _history[i] = generator(i);
        }
    }
}
