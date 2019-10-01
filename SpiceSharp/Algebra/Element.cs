using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A description of a matrix element.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class Element<T> : IFormattable where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the matrix element.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
            => Value.ToString();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="provider">The provider.</param>
        public string ToString(string format, IFormatProvider provider)
            => Value.ToString(format, provider);
    }
}
