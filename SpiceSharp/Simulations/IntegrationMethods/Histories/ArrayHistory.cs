using System;
using System.Collections.Generic;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// History of objects using an array
    /// </summary>
    public class ArrayHistory<T> : History<T>
    {
        /// <summary>
        /// Gets or sets the current value
        /// </summary>
        public override T Current
        {
            get => _history[0];
            set => _history[0] = value;
        }

        /// <summary>
        /// Gets a value in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
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
        /// Timesteps in history
        /// </summary>
        private T[] _history;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public ArrayHistory(int length)
            : base(length)
        {
            _history = new T[length];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        public ArrayHistory(int length, T defaultValue)
            : base(length)
        {
            _history = new T[length];
            for (int i = 0; i < length; i++)
                _history[i] = defaultValue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="generator">Default value generator</param>
        public ArrayHistory(int length, Func<int, T> generator)
            : base(length)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            for (int i = 0; i < length; i++)
                _history[i] = generator(i);
        }

        /// <summary>
        /// Store the current value
        /// </summary>
        public override void Cycle()
        {
            T tmp = _history[Length - 1];
            for (int i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = tmp;
        }

        /// <summary>
        /// Store a new value
        /// </summary>
        /// <param name="newValue"></param>
        public override void Store(T newValue)
        {
            // Shift the history
            for (int i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = newValue;
        }

        /// <summary>
        /// Clear the whole history with the same value
        /// </summary>
        /// <param name="value">Value</param>
        public override void Clear(T value)
        {
            for (int i = 0; i < Length; i++)
                _history[i] = value;
        }

        /// <summary>
        /// Clear the history using a generator
        /// </summary>
        /// <param name="generator">Generator</param>
        public override void Clear(Func<int, T> generator)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            for (int i = 0; i < Length; i++)
                _history[i] = generator(i);
        }

        /// <summary>
        /// Gets enumerable version
        /// </summary>
        protected override IEnumerable<T> Points => _history;
    }
}
