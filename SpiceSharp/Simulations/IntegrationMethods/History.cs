using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// History of timesteps
    /// </summary>
    public class History<T>
    {
        /// <summary>
        /// Gets or sets the current value
        /// </summary>
        public T Value
        {
            get => history[0];
            set => history[0] = value;
        }

        /// <summary>
        /// Gets a value in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new CircuitException("Invalid index {0}".FormatString(index));
                return history[index];
            }
        }

        /// <summary>
        /// Gets the number of timesteps stored
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Timesteps in history
        /// </summary>
        T[] history;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public History(int length)
        {
            if (length < 1)
                throw new CircuitException("Not enough points");
            Length = length;
            history = new T[length];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        public History(int length, T defaultValue)
            : this(length)
        {
            for (int i = 0; i < length; i++)
                history[i] = defaultValue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="generator">Default value generator</param>
        public History(int length, Func<int, T> generator)
            : this(length)
        {
            for (int i = 0; i < length; i++)
                history[i] = generator(i);
        }

        /// <summary>
        /// Store the current value
        /// </summary>
        public void Store()
        {
            // Shift the history
            T tmp = history[Length - 1];
            for (int i = Length - 1; i > 0; i--)
                history[i] = history[i - 1];
            history[0] = tmp;
        }

        /// <summary>
        /// Store a new value
        /// </summary>
        /// <param name="newValue"></param>
        public void Store(T newValue)
        {
            // Shift the history
            for (int i = Length - 1; i > 0; i--)
                history[i] = history[i - 1];
            history[0] = newValue;
        }

        /// <summary>
        /// Clear the whole history with the same value
        /// </summary>
        /// <param name="value">Value</param>
        public void Clear(T value)
        {
            for (int i = 0; i < Length; i++)
                history[i] = value;
        }

        /// <summary>
        /// Clear the history using a generator
        /// </summary>
        /// <param name="generator">Generator</param>
        public void Clear(Func<int, T> generator)
        {
            for (int i = 0; i < Length; i++)
                history[i] = generator(i);
        }
    }
}
