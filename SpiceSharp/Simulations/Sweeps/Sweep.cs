using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a sweep.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class Sweep<T>
    {
        /// <summary>
        /// Gets an enumeration of the points in the sweep.
        /// </summary>
        /// <value>
        /// The points in the sweep.
        /// </value>
        public abstract IEnumerable<T> Points { get; }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>
        /// The current value.
        /// </value>
        public T Current { get; protected set; }

        /// <summary>
        /// Gets the initial value of the sweep.
        /// </summary>
        /// <value>
        /// The initial value.
        /// </value>
        [ParameterName("start"), ParameterInfo("Initial value")]
        public T Initial { get; protected set; }

        /// <summary>
        /// Gets the final value of the sweep.
        /// </summary>
        /// <value>
        /// The final value.
        /// </value>
        [ParameterName("final"), ParameterInfo("Final value")]
        public T Final { get; protected set; }

        /// <summary>
        /// Gets the number of points in the sweep.
        /// </summary>
        /// <value>
        /// The number of points in the sweep.
        /// </value>
        [ParameterName("steps"), ParameterName("n"), ParameterInfo("Number of steps")]
        public int Count { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sweep{T}"/> class.
        /// </summary>
        protected Sweep()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sweep{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="count">The number of points.</param>
        /// <exception cref="ArgumentException">Invalid count</exception>
        protected Sweep(T initial, T final, int count)
        {
            if (count < 0)
                throw new ArgumentException("Invalid count");

            Initial = initial;
            Final = final;
            Count = count;
        }
    }
}
