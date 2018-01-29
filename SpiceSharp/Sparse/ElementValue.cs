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
    }
}
