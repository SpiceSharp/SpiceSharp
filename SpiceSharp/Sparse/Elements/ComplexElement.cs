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
        public override void AssignMultiply(Complex first, Complex second) => Value = first * second;

        /// <summary>
        /// Add the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AddMultiply(Complex first, Complex second) => Value += first * second;

        /// <summary>
        /// Subtract the result of the multiplication
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public override void SubtractMultiply(Complex first, Complex second) => Value -= first * second;

        /// <summary>
        /// Multiply with a factor
        /// </summary>
        /// <param name="factor">Factor</param>
        public override void Multiply(Complex factor) => Value *= factor;

        /// <summary>
        /// Assign reciprocal
        /// </summary>
        /// <param name="denominator">Denominator</param>
        public override void AssignReciprocal(Complex denominator)
        {
            double r;
            if ((denominator.Real >= denominator.Imaginary && denominator.Real > -denominator.Imaginary) ||
                (denominator.Real < denominator.Imaginary && denominator.Real <= -denominator.Imaginary))
            {
                r = denominator.Imaginary / denominator.Real;
                Imaginary = -r * (Real = 1.0 / (denominator.Real + r * denominator.Imaginary));
            }
            else
            {
                r = denominator.Real / denominator.Imaginary;
                Real = -r * (Imaginary = -1.0 / (denominator.Imaginary + r * denominator.Real));
            }
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
