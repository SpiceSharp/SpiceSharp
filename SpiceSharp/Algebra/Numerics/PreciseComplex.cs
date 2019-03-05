using System;
using System.Globalization;
using System.Numerics;

namespace SpiceSharp.Algebra.Numerics
{
    /// <summary>
    /// A PreciseComplex number z is a number of the form z = x + yi, where x and y 
    /// are decimal numbers, and i is the imaginary unit, with the property i2= -1.
    /// @author .NET Foundation and Contributors
    /// @author Marcin Gołębiowski (modification with decimals)
    /// </summary>
    public struct PreciseComplex : IEquatable<PreciseComplex>, IFormattable
    {
        public static readonly PreciseComplex Zero = new PreciseComplex(0.0, 0.0);
        public static readonly PreciseComplex One = new PreciseComplex(1.0, 0.0);

        // Do not rename, these fields are needed for binary serialization
        private decimal m_real; // Do not rename (binary serialization)
        private decimal m_imaginary; // Do not rename (binary serialization)

        public PreciseComplex(decimal real, decimal imaginary)
        {
            m_real = real;
            m_imaginary = imaginary;
        }

        public PreciseComplex(double real, double imaginary)
        {
            m_real = new decimal(real);
            m_imaginary = new decimal(imaginary);
        }

        public PreciseComplex(double real, decimal imaginary)
        {
            m_real = new decimal(real);
            m_imaginary = imaginary;
        }

        public PreciseComplex(decimal real, double imaginary)
        {
            m_real = real;
            m_imaginary = new decimal(imaginary);
        }

        public decimal Real { get { return m_real; } }
        public decimal Imaginary { get { return m_imaginary; } }

        public static PreciseComplex Negate(PreciseComplex value)
        {
            return -value;
        }

        public static PreciseComplex Add(PreciseComplex left, PreciseComplex right)
        {
            return left + right;
        }

        public static PreciseComplex Add(PreciseComplex left, decimal right)
        {
            return left + right;
        }

        public static PreciseComplex Add(decimal left, PreciseComplex right)
        {
            return left + right;
        }

        public static PreciseComplex Subtract(PreciseComplex left, PreciseComplex right)
        {
            return left - right;
        }

        public static PreciseComplex Subtract(PreciseComplex left, decimal right)
        {
            return left - right;
        }

        public static PreciseComplex Subtract(decimal left, PreciseComplex right)
        {
            return left - right;
        }

        public static PreciseComplex Multiply(PreciseComplex left, PreciseComplex right)
        {
            return left * right;
        }

        public static PreciseComplex Multiply(PreciseComplex left, decimal right)
        {
            return left * right;
        }

        public static PreciseComplex Multiply(decimal left, PreciseComplex right)
        {
            return left * right;
        }

        public static PreciseComplex Divide(PreciseComplex dividend, PreciseComplex divisor)
        {
            return dividend / divisor;
        }

        public static PreciseComplex Divide(PreciseComplex dividend, decimal divisor)
        {
            return dividend / divisor;
        }

        public static PreciseComplex Divide(decimal dividend, PreciseComplex divisor)
        {
            return dividend / divisor;
        }

        public static PreciseComplex operator -(PreciseComplex value)  /* Unary negation of a PreciseComplex number */
        {
            return new PreciseComplex(-value.m_real, -value.m_imaginary);
        }

        public static PreciseComplex operator +(PreciseComplex left, PreciseComplex right)
        {
            return new PreciseComplex(left.m_real + right.m_real, left.m_imaginary + right.m_imaginary);
        }

        public static PreciseComplex operator +(PreciseComplex left, decimal right)
        {
            return new PreciseComplex(left.m_real + right, left.m_imaginary);
        }

        public static PreciseComplex operator +(decimal left, PreciseComplex right)
        {
            return new PreciseComplex(left + right.m_real, right.m_imaginary);
        }

        public static PreciseComplex operator -(PreciseComplex left, PreciseComplex right)
        {
            return new PreciseComplex(left.m_real - right.m_real, left.m_imaginary - right.m_imaginary);
        }

        public static PreciseComplex operator -(PreciseComplex left, decimal right)
        {
            return new PreciseComplex(left.m_real - right, left.m_imaginary);
        }

        public static PreciseComplex operator -(decimal left, PreciseComplex right)
        {
            return new PreciseComplex(left - right.m_real, -right.m_imaginary);
        }

        public static PreciseComplex operator *(PreciseComplex left, PreciseComplex right)
        {
            // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
            decimal result_realpart = (left.m_real * right.m_real) - (left.m_imaginary * right.m_imaginary);
            decimal result_imaginarypart = (left.m_imaginary * right.m_real) + (left.m_real * right.m_imaginary);
            return new PreciseComplex(result_realpart, result_imaginarypart);
        }

        public static PreciseComplex operator *(PreciseComplex left, decimal right)
        {
            return new PreciseComplex(left.m_real * right, left.m_imaginary * right);
        }

        public static PreciseComplex operator *(decimal left, PreciseComplex right)
        {
            return new PreciseComplex(left * right.m_real, left * right.m_imaginary);
        }

        public static PreciseComplex operator *(double left, PreciseComplex right)
        {
            return new PreciseComplex(new decimal(left) * right.m_real, new decimal(left) * right.m_imaginary);
        }

        public static PreciseComplex operator /(PreciseComplex left, PreciseComplex right)
        {
            // Division : Smith's formula.
            decimal a = left.m_real;
            decimal b = left.m_imaginary;
            decimal c = right.m_real;
            decimal d = right.m_imaginary;

            // Computing c * c + d * d will overflow even in cases where the actual result of the division does not overflow.
            if (Math.Abs(d) < Math.Abs(c))
            {
                decimal doc = d / c;
                return new PreciseComplex((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
            }
            else
            {
                decimal cod = c / d;
                return new PreciseComplex((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
            }
        }

        public static PreciseComplex operator /(PreciseComplex left, decimal right)
        {
            // IEEE prohibit optimizations which are value changing
            // so we make sure that behaviour for the simplified version exactly match
            // full version.
            if (right == 0)
            {
                return new PreciseComplex(decimal.Zero, decimal.Zero);
            }
            // Here the actual optimized version of code.
            return new PreciseComplex(left.m_real / right, left.m_imaginary / right);
        }

        public static PreciseComplex operator /(decimal left, PreciseComplex right)
        {
            // Division : Smith's formula.
            decimal a = left;
            decimal c = right.m_real;
            decimal d = right.m_imaginary;

            // Computing c * c + d * d will overflow even in cases where the actual result of the division does not overflow.
            if (Math.Abs(d) < Math.Abs(c))
            {
                decimal doc = d / c;
                return new PreciseComplex(a / (c + d * doc), (-a * doc) / (c + d * doc));
            }
            else
            {
                decimal cod = c / d;
                return new PreciseComplex(a * cod / (d + c * cod), -a / (d + c * cod));
            }
        }

        public static PreciseComplex Conjugate(PreciseComplex value)
        {
            // Conjugate of a PreciseComplex number: the conjugate of x+i*y is x-i*y
            return new PreciseComplex(value.m_real, -value.m_imaginary);
        }

        public static PreciseComplex Reciprocal(PreciseComplex value)
        {
            // Reciprocal of a PreciseComplex number : the reciprocal of x+i*y is 1/(x+i*y)
            if (value.m_real == 0 && value.m_imaginary == 0)
            {
                return Zero;
            }
            return One / value;
        }


        public static PreciseComplex Exp(PreciseComplex value)
        {
            double expReal = Math.Exp((double)value.Real);
            double cosImaginary = expReal * Math.Cos((double)value.Imaginary);
            double sinImaginary = expReal * Math.Sin((double)value.Imaginary);
            return new PreciseComplex(cosImaginary, sinImaginary);
        }

        public static bool operator ==(PreciseComplex left, PreciseComplex right)
        {
            return left.m_real == right.m_real && left.m_imaginary == right.m_imaginary;
        }

        public static bool operator !=(PreciseComplex left, PreciseComplex right)
        {
            return left.m_real != right.m_real || left.m_imaginary != right.m_imaginary;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PreciseComplex)) return false;
            return Equals((PreciseComplex)obj);
        }

        public bool Equals(PreciseComplex value)
        {
            return m_real.Equals(value.m_real) && m_imaginary.Equals(value.m_imaginary);
        }

        public override int GetHashCode()
        {
            int n1 = 99999997;
            int realHash = m_real.GetHashCode() % n1;
            int imaginaryHash = m_imaginary.GetHashCode();
            int finalHash = realHash ^ imaginaryHash;
            return finalHash;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real, m_imaginary);
        }

        public string ToString(string format)
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real.ToString(format, CultureInfo.CurrentCulture), m_imaginary.ToString(format, CultureInfo.CurrentCulture));
        }

        public string ToString(IFormatProvider provider)
        {
            return string.Format(provider, "({0}, {1})", m_real, m_imaginary);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return string.Format(provider, "({0}, {1})", m_real.ToString(format, provider), m_imaginary.ToString(format, provider));
        }

        public static implicit operator PreciseComplex(short value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        public static implicit operator PreciseComplex(int value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        public static implicit operator PreciseComplex(long value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        [CLSCompliant(false)]
        public static implicit operator PreciseComplex(ushort value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        [CLSCompliant(false)]
        public static implicit operator PreciseComplex(uint value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        [CLSCompliant(false)]
        public static implicit operator PreciseComplex(ulong value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        [CLSCompliant(false)]
        public static implicit operator PreciseComplex(sbyte value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        public static implicit operator PreciseComplex(byte value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0));
        }

        public static implicit operator PreciseComplex(float value)
        {
            return new PreciseComplex(value, 0.0);
        }

        public static implicit operator PreciseComplex(decimal value)
        {
            return new PreciseComplex(value, 0.0);
        }

        public static implicit operator PreciseComplex(double value)
        {
            return new PreciseComplex(new decimal(value), new decimal(0.0));
        }

        public static explicit operator PreciseComplex(Complex value)
        {
            return new PreciseComplex(value.Real, value.Imaginary);
        }
        public static explicit operator Complex(PreciseComplex value)
        {
            return new Complex((double)value.Real, (double)value.Imaginary);
        }
    }
}
