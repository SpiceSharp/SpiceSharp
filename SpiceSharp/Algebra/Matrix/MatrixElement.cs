using System;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Base class for matrix elements.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class MatrixElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the element.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets the row of the element.
        /// </summary>
        public int Row { get; protected set; }

        /// <summary>
        /// Gets the column of the element.
        /// </summary>
        public int Column { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixElement{T}"/> class.
        /// </summary>
        protected MatrixElement()
        {
            Value = default;
        }

        /// <summary>
        /// Gets the element below (same column).
        /// </summary>
        public abstract MatrixElement<T> Below { get; }

        /// <summary>
        /// Gets the element above (same column).
        /// </summary>
        public abstract MatrixElement<T> Above { get; }

        /// <summary>
        /// Gets the element on the right (same row).
        /// </summary>
        public abstract MatrixElement<T> Right { get; }

        /// <summary>
        /// Gets the element on the left (same row).
        /// </summary>
        public abstract MatrixElement<T> Left { get; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "({0}, {1}) = {2}".FormatString(Row, Column, Value);
        }
    }
}
