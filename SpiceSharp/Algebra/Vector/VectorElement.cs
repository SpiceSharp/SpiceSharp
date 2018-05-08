using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for vector elements
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class VectorElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the element
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets the index of the element
        /// </summary>
        public int Index { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected VectorElement(int index)
        {
            Index = index;
            Value = default(T);
        }

        /// <summary>
        /// Gets the next element
        /// </summary>
        public abstract VectorElement<T> Below { get; }

        /// <summary>
        /// Gets the previous element
        /// </summary>
        public abstract VectorElement<T> Above { get; }
    }
}
