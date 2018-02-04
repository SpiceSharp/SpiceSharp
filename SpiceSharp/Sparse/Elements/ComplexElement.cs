using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Element with a complex value
    /// </summary>
    public sealed class ComplexElement : Element<Complex>
    {
        /// <summary>
        /// Gets the equivalent of One
        /// </summary>
        public override Complex One => new Complex(1.0, 0.0);

        /// <summary>
        /// Gets or sets the real part
        /// </summary>
        public double Real { get; set; }

        /// <summary>
        /// Gets or sets the imaginary part
        /// </summary>
        public double Imaginary { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public override Complex Value
        {
            get => new Complex(Real, Imaginary);
            set
            {
                Real = value.Real;
                Imaginary = value.Imaginary;
            }
        }

        /// <summary>
        /// Gets the magnitude
        /// </summary>
        public override double Magnitude => Math.Abs(Real) + Math.Abs(Imaginary);

        /// <summary>
        /// Constructor
        /// </summary>
        public ComplexElement()
        {
            Value = 0.0;
            Imaginary = 0.0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="real">Real part</param>
        /// <param name="imaginary">Imaginary part</param>
        public ComplexElement(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public override void Add(Complex operand) => Value += operand;

        /// <summary>
        /// Subtract a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public override void Sub(Complex operand) => Value -= operand;

        /// <summary>
        /// Negate the value
        /// </summary>
        public override void Negate() => Value = -Value;

        /// <summary>
        /// Store the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AssignMultiply(Element<Complex> first, Element<Complex> second)
        {
            if (first == null || second == null)
            {
                Real = 0;
                Imaginary = 0;
            }
            else
            {
                var fValue = first.Value;
                var sValue = second.Value;
                Real = fValue.Real * sValue.Real - fValue.Imaginary * sValue.Imaginary;
                Imaginary = fValue.Real * sValue.Imaginary + fValue.Imaginary * sValue.Real;
            }
        }

        /// <summary>
        /// Add the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AddMultiply(Element<Complex> first, Element<Complex> second)
        {
            if (first == null || second == null)
                return;
            var fValue = first.Value;
            var sValue = second.Value;
            Real += fValue.Real * sValue.Real - fValue.Imaginary * sValue.Imaginary;
            Imaginary += fValue.Real * sValue.Imaginary + fValue.Imaginary * sValue.Real;
        }

        /// <summary>
        /// Subtract the result of the multiplication
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public override void SubtractMultiply(Element<Complex> first, Element<Complex> second)
        {
            if (first == null || second == null)
                return;
            var fValue = first.Value;
            var sValue = second.Value;
            Real -= fValue.Real * sValue.Real - fValue.Imaginary * sValue.Imaginary;
            Imaginary -= fValue.Real * sValue.Imaginary + fValue.Imaginary * sValue.Real;
        }

        /// <summary>
        /// Multiply with a factor
        /// </summary>
        /// <param name="factor">Factor</param>
        public override void Multiply(Element<Complex> factor)
        {
            if (factor == null)
                Value = 0.0;
            else
            {
                var fValue = factor.Value;
                double r = Real;
                Real = r * fValue.Real - Imaginary * fValue.Imaginary;
                Imaginary = r * fValue.Imaginary + Imaginary * fValue.Real;
            }
        }

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        /// <param name="factor">Scalar factor</param>
        public override void Scalar(double factor)
        {
            Real *= factor;
            Imaginary *= factor;
        }

        /// <summary>
        /// Assign reciprocal
        /// </summary>
        /// <param name="denominator">Denominator</param>
        public override void AssignReciprocal(Element<Complex> denominator)
        {
            if (denominator == null)
            {
                Value = double.NaN;
                return;
            }

            double r;
            Complex value = denominator.Value;
            if ((value.Real >= value.Imaginary && value.Real > -value.Imaginary) ||
                (value.Real < value.Imaginary && value.Real <= -value.Imaginary))
            {
                r = value.Imaginary / value.Real;
                Real = 1.0 / (value.Real + r * value.Imaginary);
                Imaginary = -r * Real;
            }
            else
            {
                r = value.Real / value.Imaginary;
                Imaginary = -1.0 / (value.Imaginary + r * value.Real);
                Real = -r * Imaginary;
            }
        }

        /// <summary>
        /// Check for identity multiplier
        /// </summary>
        /// <returns></returns>
        public override bool EqualsOne()
        {
            if (!Imaginary.Equals(0.0))
                return false;
            if (Math.Abs(Real).Equals(1.0))
                return true;
            return false;
        }

        /// <summary>
        /// Check if the element is 0
        /// </summary>
        /// <returns></returns>
        public override bool EqualsZero()
        {
            if (!Real.Equals(0))
                return false;
            if (!Imaginary.Equals(0))
                return false;
            return true;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Imaginary.Equals(0))
                return "{0:g5}".FormatString(Value);
            return "({0:g5}; {0:g5})".FormatString(Value);
        }
    }
}
