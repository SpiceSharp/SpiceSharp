using SpiceSharp.Attributes;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An abstract class for 
    /// </summary>
    public abstract class Sweep<T>
    {
        /// <summary>
        /// Gets an enumeration of the points in the sweep
        /// </summary>
        public abstract IEnumerable<T> Points { get; }

        /// <summary>
        /// Get the current value
        /// </summary>
        public T Current { get; protected set; }

        /// <summary>
        /// The initial value of the sweep
        /// </summary>
        [PropertyName("start"), PropertyInfo("Initial value")]
        public T Initial { get; protected set; }

        /// <summary>
        /// The final value of the sweep
        /// </summary>
        [PropertyName("final"), PropertyInfo("Final value")]
        public T Final { get; protected set; }

        /// <summary>
        /// The number of points in the sweep
        /// </summary>
        [PropertyName("steps"), PropertyName("n"), PropertyInfo("Number of steps")]
        public int Count { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Sweep()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Initial sweep value</param>
        /// <param name="final">Final sweep value</param>
        /// <param name="count">Number of points</param>
        protected Sweep(T initial, T final, int count)
        {
            Initial = initial;
            Final = final;
            Count = count;
        }
    }
}
