using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A base class for vector elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public abstract class VectorElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the element.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets the index of the element.
        /// </summary>
        public int Index { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorElement{T}"/> class.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        protected VectorElement(int index)
        {
            Index = index;
            Value = default;
        }

        /// <summary>
        /// Gets the next element.
        /// </summary>
        public abstract VectorElement<T> Below { get; }

        /// <summary>
        /// Gets the previous element.
        /// </summary>
        public abstract VectorElement<T> Above { get; }
    }
}
