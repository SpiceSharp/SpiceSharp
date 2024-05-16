using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpiceSharp.Simulations.Variables
{
    /// <summary>
    /// SI units.
    /// </summary>
    /// <seealso cref="IEquatable{T}"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct SIUnits : IEquatable<SIUnits>
    {
        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing a second (time).
        /// </summary>
        public const ulong Second = ((ulong)Fraction.One) << 0;

        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing a meter (distance).
        /// </summary>
        public const ulong Meter = ((ulong)Fraction.One) << 8;

        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing a kilogram (weight).
        /// </summary>
        public const ulong Kilogram = ((ulong)Fraction.One) << 16;

        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing an Ampere (current).
        /// </summary>
        public const ulong Ampere = ((ulong)Fraction.One) << 24;

        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing a Kelvin (temperature).
        /// </summary>
        public const ulong Kelvin = ((ulong)Fraction.One) << 32;

        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing a mole (amount of substance).
        /// </summary>
        public const ulong Mole = ((ulong)Fraction.One) << 40;

        /// <summary>
        /// Binary code for an <see cref="SIUnits"/> representing a Candela (luminous intensity).
        /// </summary>
        public const ulong Candela = ((ulong)Fraction.One) << 48;

        [FieldOffset(0)] private readonly ulong _units;
        [FieldOffset(0)] private readonly Fraction _s;
        [FieldOffset(1)] private readonly Fraction _m;
        [FieldOffset(2)] private readonly Fraction _kg;
        [FieldOffset(3)] private readonly Fraction _a;
        [FieldOffset(4)] private readonly Fraction _k;
        [FieldOffset(5)] private readonly Fraction _mol;
        [FieldOffset(6)] private readonly Fraction _cd;

        /// <summary>
        /// Initializes a new instance of the <see cref="SIUnits"/> struct.
        /// </summary>
        /// <param name="s">The exponent for seconds.</param>
        /// <param name="m">The exponent for meters.</param>
        /// <param name="kg">The exponent for kilograms.</param>
        /// <param name="a">The exponent for amperes.</param>
        /// <param name="k">The exponent for kelvin.</param>
        /// <param name="mol">The exponent for moles.</param>
        /// <param name="cd">The exponent for candelas.</param>
        public SIUnits(Fraction s, Fraction m, Fraction kg, Fraction a, Fraction k, Fraction mol, Fraction cd)
            : this()
        {
            _s = s;
            _m = m;
            _kg = kg;
            _a = a;
            _k = k;
            _mol = mol;
            _cd = cd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SIUnits"/> struct.
        /// </summary>
        /// <param name="units">The units.</param>
        private SIUnits(ulong units) : this()
        {
            _units = units;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is SIUnits units)
                return Equals(units);
            return false;
        }

        /// <summary>
        /// Determines whether the specified units are equal.
        /// </summary>
        /// <param name="units">The units.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="SIUnits"/> is equal to this instance; otherwise <c>false</c>.
        /// </returns>
        public readonly bool Equals(SIUnits units) => _units == units._units;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode() => _units.GetHashCode();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override readonly string ToString()
        {
            var sb = new StringBuilder(16);
            if (_m != 0)
                sb.Append(FormatUnit("m", _m));
            if (_s != 0)
                sb.Append(FormatUnit("s", _s));
            if (_kg != 0)
                sb.Append(FormatUnit("kg", _kg));
            if (_a != 0)
                sb.Append(FormatUnit("A", _a));
            if (_k != 0)
                sb.Append(FormatUnit("K", _k));
            if (_mol != 0)
                sb.Append(FormatUnit("mol", _mol));
            if (_cd != 0)
                sb.Append(FormatUnit("cd", _cd));
            return sb.ToString();
        }

        /// <summary>
        /// Raises the units to a power.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The result.</returns>
        public readonly SIUnits Pow(Fraction exponent)
        {
            return new SIUnits(
                _s * exponent,
                _m * exponent,
                _kg * exponent,
                _a * exponent,
                _k * exponent,
                _mol * exponent,
                _cd * exponent
                );
        }

        private static string FormatUnit(string unit, Fraction fraction)
        {
            if (fraction == 1)
                return unit;
            if (fraction.Denominator == 1)
                return "{0}^{1}".FormatString(unit, fraction);
            return "{0}^({1})".FormatString(unit, fraction);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(SIUnits left, SIUnits right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(SIUnits left, SIUnits right) => !left.Equals(right);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static SIUnits operator *(SIUnits left, SIUnits right)
        {
            return new SIUnits(
                left._s + right._s,
                left._m + right._m,
                left._kg + right._k,
                left._a + right._a,
                left._k + right._k,
                left._mol + right._mol,
                left._cd + right._cd
                );
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static SIUnits operator /(SIUnits left, SIUnits right)
        {
            return new SIUnits(
                left._s - right._s,
                left._m - right._m,
                left._kg - right._k,
                left._a - right._a,
                left._k - right._k,
                left._mol - right._mol,
                left._cd - right._cd
                );
        }
    }
}
