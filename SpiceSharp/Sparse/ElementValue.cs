using System.Numerics;
using System.Runtime.InteropServices;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A value for a matrix element
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ElementValue
    {
        /// <summary>
        /// The real value
        /// </summary>
        [FieldOffset(0)]
        public double Real;

        /// <summary>
        /// The imaginary value
        /// </summary>
        [FieldOffset(8)]
        public double Imag;

        /// <summary>
        /// The complex representation
        /// </summary>
        [FieldOffset(0)]
        public Complex Cplx;

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
            if (Imag == 0.0)
                return "{0}".FormatString(Real);
            else
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
    }
}
