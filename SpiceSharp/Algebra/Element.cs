using System;
using System.Linq.Expressions;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A description of a matrix element.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class Element<T> : IFormattable where T : IFormattable
    {
        /// <summary>
        /// Generic addition.
        /// </summary>
        protected readonly static Func<T, T, T> Addition;

        /// <summary>
        /// Generic subtraction.
        /// </summary>
        protected readonly static Func<T, T, T> Subtraction;

        /// <summary>
        /// Initializes the <see cref="Element{T}"/> class.
        /// </summary>
        static Element()
        {
            var a = Expression.Parameter(typeof(T), "a");
            var b = Expression.Parameter(typeof(T), "b");
            Addition = Expression.Lambda<Func<T, T, T>>(Expression.Add(a, b), a, b).Compile();
            Subtraction = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(a, b), a, b).Compile();
        }

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

        /// <summary>
        /// Adds the specified value to the element.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Add(T value) => Value = Addition(Value, value);

        /// <summary>
        /// Subtracts the specified value from the element.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Subtract(T value) => Value = Subtraction(Value, value);
    }
}
