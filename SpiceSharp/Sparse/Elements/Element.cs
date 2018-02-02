using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Represents an element in a matrix
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class Element<T> where T : struct
    {
        /// <summary>
        /// Gets or sets the value of the element
        /// </summary>
        public abstract T Value { get; set; }

        /// <summary>
        /// Gets the magnitude of the element
        /// </summary>
        public abstract double Magnitude { get; }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public abstract void Add(T operand);

        /// <summary>
        /// Subtract a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public abstract void Sub(T operand);

        /// <summary>
        /// Negate the value
        /// </summary>
        public abstract void Negate();

        /// <summary>
        /// Store the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public abstract void AssignMultiply(T first, T second);

        /// <summary>
        /// Add the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public abstract void AddMultiply(T first, T second);

        /// <summary>
        /// Subtract the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public abstract void SubtractMultiply(T first, T second);

        /// <summary>
        /// Multiply with a factor
        /// </summary>
        /// <param name="factor">Factor</param>
        public abstract void Multiply(T factor);

        /// <summary>
        /// Assign reciprocal
        /// </summary>
        /// <param name="denominator">Denominator</param>
        public abstract void AssignReciprocal(T denominator);

        /// <summary>
        /// Copy values from another source
        /// </summary>
        /// <param name="source">Source</param>
        public virtual void CopyFrom(Element<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Value = source.Value;
        }

        /// <summary>
        /// Implicit conversion to the base type
        /// </summary>
        /// <param name="element">Element</param>
        public static implicit operator T(Element<T> element)
        {
            if (element == null)
                return default(T);
            return element.Value;
        }

        /// <summary>
        /// Override hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Value.GetHashCode();
    }
}
