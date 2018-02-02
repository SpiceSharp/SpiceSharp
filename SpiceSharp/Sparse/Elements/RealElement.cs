using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Element with a real value
    /// </summary>
    public sealed class RealElement : Element<double>
    {
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public override double Value { get; set; }

        /// <summary>
        /// Magnitude (sum of absolute values)
        /// </summary>
        public override double Magnitude => Math.Abs(Value);

        /// <summary>
        /// Constructor
        /// </summary>
        public RealElement()
        {
            Value = 0.0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public RealElement(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public override void Add(double operand) => Value += operand;

        /// <summary>
        /// Subtract a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public override void Sub(double operand) => Value -= operand;

        /// <summary>
        /// Negate the value
        /// </summary>
        public override void Negate() => Value = -Value;

        /// <summary>
        /// Store the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AssignMultiply(double first, double second) => Value = first * second;

        /// <summary>
        /// Add the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AddMultiply(double first, double second) => Value += first * second;

        /// <summary>
        /// Subtract the result of the multiplication
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public override void SubtractMultiply(double first, double second) => Value -= first * second;

        /// <summary>
        /// Multiply with a factor
        /// </summary>
        /// <param name="factor">Factor</param>
        public override void Multiply(double factor) => Value *= factor;

        /// <summary>
        /// Assign reciprocal
        /// </summary>
        /// <param name="denominator">Denominator</param>
        public override void AssignReciprocal(double denominator) => Value = 1.0 / denominator;

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0:g5}".FormatString(Value);
        }
    }
}
