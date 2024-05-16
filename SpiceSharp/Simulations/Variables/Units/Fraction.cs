using System;

namespace SpiceSharp.Simulations.Variables
{
    /// <summary>
    /// Represents a small fraction. The numerator can range from -16 to 15, and
    /// the denominator can range from 1 to 8
    /// </summary>
    public struct Fraction : IEquatable<Fraction>, IFormattable
    {
        private readonly sbyte _fraction;

        /// <summary>
        /// One in a byte.
        /// </summary>
        public const sbyte One = 0b00001_000;

        /// <summary>
        /// Gets the numerator.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        /// <remarks>
        /// The numerator can have a value ranging from -16 to +15.
        /// </remarks>
        public readonly sbyte Numerator => (sbyte)(_fraction >> 3);

        /// <summary>
        /// Gets the denominator.
        /// </summary>
        /// <value>
        /// The denominator.
        /// </value>
        /// <remarks>
        /// The denominator can have a value ranging from 1 to 8.
        /// </remarks>
        public readonly sbyte Denominator => (sbyte)((_fraction & 0b00000_111) + 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Fraction"/> struct.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <exception cref="DivideByZeroException">Thrown if the denominator is zero.</exception>
        /// <exception cref="ArgumentException">Thrown if the numerator is not in the range -16 to +15, or if the denominator is not in the range 1 to 8.</exception>
        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0)
                throw new DivideByZeroException();
            if (numerator != 0)
            {
                int gcd = Gcd(numerator, denominator);
                numerator /= gcd;
                denominator /= gcd;
            }
            _fraction = (sbyte)((numerator << 3) | ((denominator - 1) & 0x07));

            // Double-check that the numerator and denominator are represented correctly
            if (Numerator != numerator || Denominator != denominator)
                throw new ArgumentException(Properties.Resources.Units_InvalidExponent.FormatString(numerator, denominator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fraction"/> struct.
        /// </summary>
        /// <param name="code">The code.</param>
        public Fraction(sbyte code)
        {
            _fraction = code;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode() => _fraction.GetHashCode();

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is Fraction fraction)
                return Equals(fraction);
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public readonly bool Equals(Fraction other) => _fraction == other._fraction;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override readonly string ToString()
        {
            if (Denominator != 1)
                return "{0}/{1}".FormatString(Numerator, Denominator);
            return Numerator.ToString();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (Denominator != 1)
                return Numerator.ToString(format, formatProvider) + "/" + Denominator.ToString(format, formatProvider);
            return Numerator.ToString(format, formatProvider);
        }

        private static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                int t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Fraction"/> to <see cref="sbyte"/>.
        /// </summary>
        /// <param name="fraction">The fraction.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator sbyte(Fraction fraction) => fraction._fraction;

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> to <see cref="Fraction"/>. The denominator is 1.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Fraction(int numerator) => new(numerator, 1);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Fraction left, Fraction right) => !left.Equals(right);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator *(Fraction left, Fraction right) => new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator /(Fraction left, Fraction right) => new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator +(Fraction left, Fraction right) => new(left.Numerator * right.Denominator + left.Denominator * right.Numerator, left.Denominator * right.Denominator);

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator -(Fraction left, Fraction right) => new(left.Numerator * right.Denominator - left.Denominator * right.Numerator, left.Denominator * right.Denominator);
    }
}
