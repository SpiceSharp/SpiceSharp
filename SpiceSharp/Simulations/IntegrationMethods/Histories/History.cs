using System;
using System.Collections;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// History of timesteps
    /// </summary>
    public abstract class History<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets or sets the current value
        /// </summary>
        public abstract T Current { get; set; }

        /// <summary>
        /// Gets a value in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public abstract T this[int index] { get; }

        /// <summary>
        /// Gets the number of timesteps stored
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        protected History(int length)
        {
            if (length < 1)
                throw new CircuitException("Not enough points");
            Length = length;
        }

        /// <summary>
        /// Cycles through history
        /// </summary>
        public abstract void Cycle();

        /// <summary>
        /// Store a new value
        /// </summary>
        /// <param name="newValue"></param>
        public abstract void Store(T newValue);

        /// <summary>
        /// Clear the whole history with the same value
        /// </summary>
        /// <param name="value">Value</param>
        public abstract void Clear(T value);

        /// <summary>
        /// Clear the history using a generator
        /// </summary>
        /// <param name="generator">Generator</param>
        public abstract void Clear(Func<int, T> generator);

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => Points.GetEnumerator();

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => Points.GetEnumerator();

        /// <summary>
        /// Get all points in the history
        /// </summary>
        protected abstract IEnumerable<T> Points { get; }
    }
}
