using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Represents an element in a matrix
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class Element<T> : IElement
    {
        /// <summary>
        /// Gets the equivalent of 1.0 for the element
        /// </summary>
        public abstract T One { get; }

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
        public abstract void AssignMultiply(Element<T> first, Element<T> second);

        /// <summary>
        /// Add the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public abstract void AddMultiply(Element<T> first, Element<T> second);

        /// <summary>
        /// Subtract the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public abstract void SubtractMultiply(Element<T> first, Element<T> second);

        /// <summary>
        /// Multiply with a factor
        /// </summary>
        /// <param name="factor">Factor</param>
        public abstract void Multiply(Element<T> factor);

        /// <summary>
        /// Multiply with a scalar
        /// </summary>
        /// <param name="factor">Scalar factor</param>
        public abstract void Scalar(double factor);

        /// <summary>
        /// Assign reciprocal
        /// </summary>
        /// <param name="denominator">Denominator</param>
        public abstract void AssignReciprocal(Element<T> denominator);

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
        /// Check if the value equals 1.0 or -1.0
        /// Used for finding identity multipliers in the matrix
        /// </summary>
        /// <returns></returns>
        public abstract bool EqualsOne();

        /// <summary>
        /// Check if the value equals 0.0
        /// </summary>
        /// <returns></returns>
        public abstract bool EqualsZero();

        /// <summary>
        /// Clear the element value
        /// </summary>
        public virtual void Clear() => Value = default(T);

        /// <summary>
        /// Override hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Value.GetHashCode();
    }
}
