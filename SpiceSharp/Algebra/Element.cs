using System;
using System.Linq.Expressions;
using System.Numerics;

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
        public static Func<T, T, T> Addition { get; set; }

        /// <summary>
        /// Generic subtraction.
        /// </summary>
        public static Func<T, T, T> Subtraction { get; set; }

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
        public void Add(T value)
        {
            Addition ??= CompileDefaultAddition();
            Value = Addition(Value, value);
        }

        /// <summary>
        /// Subtracts the specified value from the element.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Subtract(T value)
        {
            Subtraction ??= CompileDefaultSubtraction();
            Value = Subtraction(Value, value);
        }

        /// <summary>
        /// Create a default addition of the generic type.
        /// </summary>
        /// <returns>The function that describes the addition.</returns>
        private static Func<T, T, T> CompileDefaultAddition()
        {
            Func<T, T, T> result = null;
            if (typeof(T) == typeof(double))
            {
                Element<double>.Addition = (a, b) => a + b;
                result = Addition;
            }
            if (typeof(T) == typeof(Complex))
            {
                Element<Complex>.Addition = (a, b) => a + b;
                result = Addition;
            }
            if (result == null)
            {
                var a = Expression.Parameter(typeof(T));
                var b = Expression.Parameter(typeof(T));
                result = Expression.Lambda<Func<T, T, T>>(Expression.Add(a, b), a, b).Compile();
            }

            // If we failed after all this, let's throw an exception
            if (result == null)
                throw new SpiceSharpException(Properties.Resources.Element_DefaultMethodNotCreated);
            return result;
        }

        /// <summary>
        /// Creates a default subtraction of the generic type.
        /// </summary>
        /// <returns>The function that describes subtraction.</returns>
        private static Func<T, T, T> CompileDefaultSubtraction()
        {
            Func<T, T, T> result = null;
            if (typeof(T) == typeof(double))
            {
                Element<double>.Subtraction = (a, b) => a - b;
                result = Subtraction;
            }
            if (typeof(T) == typeof(Complex))
            {
                Element<Complex>.Subtraction = (a, b) => a - b;
                result = Subtraction;
            }
            if (result == null)
            {
                var a = Expression.Parameter(typeof(T));
                var b = Expression.Parameter(typeof(T));
                result = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(a, b), a, b).Compile();
            }

            // If we failed after all this, let's throw an exception
            if (result == null)
                throw new SpiceSharpException(Properties.Resources.Element_DefaultMethodNotCreated);
            return result;
        }
    }
}
