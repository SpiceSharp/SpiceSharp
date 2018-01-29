using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A value for a matrix element
    /// </summary>
    public struct ElementValue
    {
        /// <summary>
        /// The real value
        /// </summary>
        public double Real { get; set; }

        /// <summary>
        /// The imaginary value
        /// </summary>
        public double Imag { get; set; }

        /// <summary>
        /// The complex representation
        /// </summary>
        public Complex Cplx
        {
            get
            {
                return new Complex(Real, Imag);
            }
            set
            {
                Real = value.Real;
                Imag = value.Imaginary;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="re">Real part</param>
        /// <param name="im">Imaginary part</param>
        public ElementValue(double re, double im) : this()
        {
            Real = re;
            Imag = im;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cplx">Complex number</param>
        public ElementValue(Complex cplx) : this()
        {
            Cplx = cplx;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Imag.Equals(0.0))
                return "{0}".FormatString(Real);
            return "({0}; {1})".FormatString(Real, Imag);
        }

        /// <summary>
        /// Convert value implicitely to a double
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator double(ElementValue value) => value.Real;

        /// <summary>
        /// Convert value implicitely to a complex value
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Complex(ElementValue value) => value.Cplx;

        /// <summary>
        /// Magnitude (sum of absolute values)
        /// </summary>
        public double Magnitude => Math.Abs(Real) + Math.Abs(Imag);

        /// <summary>
        /// Copy from another value
        /// </summary>
        /// <param name="value">Value</param>
        public void CopyFrom(ElementValue value)
        {
            Real = value.Real;
            Imag = value.Imag;
        }

        /// <summary>
        /// Negate the value
        /// </summary>
        public void Negate()
        {
            Real = -Real;
            Imag = -Imag;
        }

        /// <summary>
        /// Assign the multiplication of two elements
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void CopyMultiply(ElementValue first, ElementValue second)
        {
            Real = first.Real * second.Real - first.Imag * second.Imag;
            Imag = first.Real * second.Imag + first.Imag * second.Real;
        }

        /// <summary>
        /// Multiply two values and adds the result
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void AddMultiply(ElementValue first, ElementValue second)
        {
            Real += first.Real * second.Real - first.Imag * second.Imag;
            Imag += first.Real * second.Imag + first.Imag * second.Real;
        }

        /// <summary>
        /// Multiply two values and subtract the result
        /// </summary>
        /// <param name="first">First argument</param>
        /// <param name="second">Second argument</param>
        public void SubtractMultiply(ElementValue first, ElementValue second)
        {
            Real -= first.Real * second.Real - first.Imag * second.Imag;
            Imag -= first.Real * second.Imag + first.Imag * second.Real;
        }

        /// <summary>
        /// Multiply with another factor
        /// </summary>
        /// <param name="factor">factor</param>
        public void Multiply(ElementValue factor)
        {
            double toReal = Real;
            Real = toReal * factor.Real - Imag * factor.Imag;
            Imag = toReal * factor.Imag + Imag * factor.Real;
        }

        /// <summary>
        /// Assign
        /// </summary>
        /// <param name="den">Denominator</param>
        public void CopyReciprocal(ElementValue den)
        {
            double r;
            if ((den.Real >= den.Imag && den.Real > -den.Imag) ||
                (den.Real < den.Imag && den.Real <= -den.Imag))
            {
                r = den.Imag / den.Real;
                Imag = -r * (Real = 1.0 / (den.Real + r * den.Imag));
            }
            else
            {
                r = den.Real / den.Imag;
                Real = -r * (Imag = -1.0 / (den.Imag + r * den.Real));
            }
        }
    }
}
