using System;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Class for matrix elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Element<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the element
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets the row of the element
        /// </summary>
        public int Row { get; protected set; }

        /// <summary>
        /// Gets the column of the element
        /// </summary>
        public int Column { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Element()
        {
            Value = default;
        }

        /// <summary>
        /// Gets the element below (same column)
        /// </summary>
        public abstract Element<T> Below { get; }

        /// <summary>
        /// Gets the element above (same column)
        /// </summary>
        public abstract Element<T> Above { get; }

        /// <summary>
        /// Gets the element on the right
        /// </summary>
        public abstract Element<T> Right { get; }

        /// <summary>
        /// Gets the element on the left
        /// </summary>
        public abstract Element<T> Left { get; }
    }
}
