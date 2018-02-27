using System;
using System.Collections.Generic;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for matrix elements
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class MatrixElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the element
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Test for zero
        /// </summary>
        public bool IsZero { get => EqualityComparer<T>.Default.Equals(Value, default); }

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
        protected MatrixElement()
        {
            Value = default;
        }

        /// <summary>
        /// Gets the element below (same column)
        /// </summary>
        public abstract MatrixElement<T> Below { get; }

        /// <summary>
        /// Gets the element above (same column)
        /// </summary>
        public abstract MatrixElement<T> Above { get; }

        /// <summary>
        /// Gets the element on the right
        /// </summary>
        public abstract MatrixElement<T> Right { get; }

        /// <summary>
        /// Gets the element on the left
        /// </summary>
        public abstract MatrixElement<T> Left { get; }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "({0}, {1}) = {2}".FormatString(Row, Column, Value);
        }
    }
}
