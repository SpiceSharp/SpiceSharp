using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A value for a matrix element
    /// As opposed to Complex, Real and Imaginary can be worked with separately
    /// </summary>
    public class ElementValue
    {
        /// <summary>
        /// The real value
        /// </summary>
        public double Real { get; set; }

        /// <summary>
        /// The imaginary value
        /// </summary>
        public double Imaginary { get; set; }

        /// <summary>
        /// The complex representation
        /// </summary>
        public Complex Complex
        {
            get
            {
                return new Complex(Real, Imaginary);
            }
            set
            {
                Real = value.Real;
                Imaginary = value.Imaginary;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementValue()
        {
            Real = 0;
            Imaginary = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="real">Real part</param>
        /// <param name="imaginary">Imaginary part</param>
        public ElementValue(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="complexValue">Complex number</param>
        public ElementValue(Complex complexValue)
        {
            Complex = complexValue;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Imaginary.Equals(0.0))
                return "{0}".FormatString(Real);
            return "({0}; {1})".FormatString(Real, Imaginary);
        }

        /// <summary>
        /// Convert value implicitely to a double
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator double(ElementValue value)
        {
            if (value == null)
                return double.NaN;
            return value.Real;
        }

        /// <summary>
        /// Convert value implicitely to a complex value
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Complex(ElementValue value)
        {
            if (value == null)
                return new Complex(double.NaN, double.NaN);
            return value.Complex;
        }

        /// <summary>
        /// Magnitude (sum of absolute values)
        /// </summary>
        public double Magnitude => Math.Abs(Real) + Math.Abs(Imaginary);

        /// <summary>
        /// Copy from another value
        /// </summary>
        /// <param name="value">Value</param>
        public void CopyFrom(ElementValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Real = value.Real;
            Imaginary = value.Imaginary;
        }

        /// <summary>
        /// Negate the value
        /// </summary>
        public void Negate()
        {
            Real = -Real;
            Imaginary = -Imaginary;
        }

        /// <summary>
        /// Assign the multiplication of two elements
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void CopyMultiply(ElementValue first, ElementValue second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            Real = first.Real * second.Real - first.Imaginary * second.Imaginary;
            Imaginary = first.Real * second.Imaginary + first.Imaginary * second.Real;
        }

        /// <summary>
        /// Multiply two values and adds the result
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void AddMultiply(ElementValue first, ElementValue second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            Real += first.Real * second.Real - first.Imaginary * second.Imaginary;
            Imaginary += first.Real * second.Imaginary + first.Imaginary * second.Real;
        }

        /// <summary>
        /// Multiply two values and subtract the result
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void SubtractMultiply(ElementValue first, ElementValue second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            Real -= first.Real * second.Real - first.Imaginary * second.Imaginary;
            Imaginary -= first.Real * second.Imaginary + first.Imaginary * second.Real;
        }

        /// <summary>
        /// Multiply with another factor
        /// </summary>
        /// <param name="factor">factor</param>
        public void Multiply(ElementValue factor)
        {
            if (factor == null)
                throw new ArgumentNullException(nameof(factor));

            double toReal = Real;
            Real = toReal * factor.Real - Imaginary * factor.Imaginary;
            Imaginary = toReal * factor.Imaginary + Imaginary * factor.Real;
        }

        /// <summary>
        /// Assign
        /// </summary>
        /// <param name="den">Denominator</param>
        public void CopyReciprocal(ElementValue den)
        {
            if (den == null)
                throw new ArgumentNullException(nameof(den));

            double r;
            if ((den.Real >= den.Imaginary && den.Real > -den.Imaginary) ||
                (den.Real < den.Imaginary && den.Real <= -den.Imaginary))
            {
                r = den.Imaginary / den.Real;
                Imaginary = -r * (Real = 1.0 / (den.Real + r * den.Imaginary));
            }
            else
            {
                r = den.Real / den.Imaginary;
                Real = -r * (Imaginary = -1.0 / (den.Imaginary + r * den.Real));
            }
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is ElementValue ev)
            {
                if (Real.Equals(ev.Real) && Imaginary.Equals(ev.Imaginary))
                    return true;
                return false;
            }
            if (obj is Complex c)
            {
                if (Real.Equals(c.Real) && Imaginary.Equals(c.Imaginary))
                    return true;
                return false;
            }
            return false;
        }

        /// <summary>
        /// Hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Complex.GetHashCode();
        }
    }
}
