using System;
using System.Linq.Expressions;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A description of a matrix element.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class Element<T>
    {
        /// <summary>
        /// Generic addition.
        /// </summary>
        protected readonly static Func<T, T, T> Addition = CompileDefaultAddition();

        /// <summary>
        /// Generic subtraction.
        /// </summary>
        protected readonly static Func<T, T, T> Subtraction = CompileDefaultSubtraction();

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
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Adds the specified value to the element.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(T value) => Value = Addition(Value, value);

        /// <summary>
        /// Subtracts the specified value from the element.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Subtract(T value) => Value = Subtraction(Value, value);

        private static Func<T, T, T> CompileDefaultAddition()
        {
            var a = Expression.Parameter(typeof(T));
            var b = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, T, T>>(Expression.Add(a, b), a, b).Compile();
        }
        private static Func<T, T, T> CompileDefaultSubtraction()
        {
            var a = Expression.Parameter(typeof(T));
            var b = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, T, T>>(Expression.Subtract(a, b), a, b).Compile();
        }
    }
}
