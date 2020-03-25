
using SpiceSharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes the units of a variable. It is expressed in SI units.
    /// The struct is interchangable with a <see cref="ulong"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public partial struct Units
    {


        /// <summary>
        /// The identifier.
        /// </summary>
        [FieldOffset(0)]
        private readonly ulong _id; // All units together

        /// <summary>
        /// Time (Seconds).
        /// </summary>
        [FieldOffset(_sOffset / 8)]
        public readonly sbyte Seconds;

        /// <summary>
        /// Distance (Meters).
        /// </summary>
        [FieldOffset(_mOffset / 8)]
        public readonly sbyte Meters;

        /// <summary>
        /// Weight (Kilograms).
        /// </summary>
        [FieldOffset(_kgOffset / 8)]
        public readonly sbyte Kilograms;

        /// <summary>
        /// Current (Amperes).
        /// </summary>
        [FieldOffset(_aOffset / 8)]
        public readonly sbyte Amperes;

        /// <summary>
        /// Temperature (Kelvin).
        /// </summary>
        [FieldOffset(_kOffset / 8)]
        public readonly sbyte Kelvins;

        /// <summary>
        /// Amount of substance (Mole).
        /// </summary>
        [FieldOffset(_molOffset / 8)]
        public readonly sbyte Moles;

        /// <summary>
        /// Luminous intensity (Candela).
        /// </summary>
        [FieldOffset(_cdOffset / 8)]
        public readonly sbyte Candelas;

        /// <summary>
        /// Flags for alternate
        /// </summary>
        [FieldOffset(_flagOffset / 8)]
        public readonly Alternate Flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Units"/> struct.
        /// </summary>
        /// <param name="s">The exponent for seconds.</param>
        /// <param name="m">The exponent for meters.</param>
        /// <param name="kg">The exponent for kilograms.</param>
        /// <param name="a">The exponent for amperes.</param>
        /// <param name="k">The exponent for kelvin.</param>
        /// <param name="mol">The exponent for mol.</param>
        /// <param name="cd">The exponent for candela.</param>
        /// <param name="flags">Extra flags when using alternate units.</param>
        private Units(sbyte s, sbyte m, sbyte kg, sbyte a, sbyte k, sbyte mol, sbyte cd, Alternate flags)
            : this()
        {
            Seconds = s;
            Meters = m;
            Kilograms = kg;
            Amperes = a;
            Kelvins = k;
            Moles = mol;
            Candelas = cd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Units"/> struct.
        /// </summary>
        /// <param name="all">All.</param>
        private Units(ulong all)
            : this()
        {
            _id = all;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Units unit)
                return unit._id == _id;
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => _id.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Units left, Units right) => left._id == right._id;

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Units left, Units right) => left._id != right._id;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (Display.TryGetValue(this, out var display))
                 return display;

            // General representation
            var result = new List<string>(7);
            if (Seconds != 0)
                result.Add("s" + (Seconds != 1 ? Seconds.ToString() : ""));
            if (Meters != 0)
                result.Add("m" + (Meters != 1 ? Meters.ToString() : ""));
            if (Kilograms != 0)
                result.Add("kg" + (Kilograms != 1 ? Kilograms.ToString() : ""));
            if (Amperes != 0)
                result.Add("A" + (Amperes != 1 ? Amperes.ToString() : ""));
            if (Kelvins != 0)
            {
                if (Flags == Alternate.Celsius)
                    result.Add("\u2103C" + (Kelvins != 1 ? Kelvins.ToString() : ""));
                else
                    result.Add("K" + (Kelvins != 1 ? Kelvins.ToString() : ""));
            }
            if (Moles != 0)
                result.Add("mol" + (Moles != 1 ? Moles.ToString() : ""));
            if (Candelas != 0)
                result.Add("cd" + (Candelas != 1 ? Candelas.ToString() : ""));
            return string.Join("*", result);
        }

        /// <summary>
        /// Unit multiplication.
        /// </summary>
        /// <param name="left">Left factor.</param>
        /// <param name="right">Right factor.</param>
        /// <returns>The multiplied units.</returns>
        public static Units operator *(Units left, Units right)
        {
            if (left.Flags != right.Flags)
                throw new UnitsNotMatchedException(left, right);
            return new Units(
                (sbyte)(left.Seconds + right.Seconds),
                (sbyte)(left.Meters + right.Meters),
                (sbyte)(left.Kilograms + right.Kilograms),
                (sbyte)(left.Amperes + right.Amperes),
                (sbyte)(left.Kelvins + right.Kelvins),
                (sbyte)(left.Moles + right.Moles),
                (sbyte)(left.Candelas + right.Candelas),
                left.Flags
                );
        }

        /// <summary>
        /// Unit division.
        /// </summary>
        /// <param name="left">Numerator.</param>
        /// <param name="right">Denominator.</param>
        /// <returns>The divided units.</returns>
        public static Units operator /(Units left, Units right)
        {
            if (left.Flags != right.Flags)
                throw new UnitsNotMatchedException(left, right);
            return new Units(
                (sbyte)(left.Seconds - right.Seconds),
                (sbyte)(left.Meters - right.Meters),
                (sbyte)(left.Kilograms - right.Kilograms),
                (sbyte)(left.Amperes - right.Amperes),
                (sbyte)(left.Kelvins - right.Kelvins),
                (sbyte)(left.Moles - right.Moles),
                (sbyte)(left.Candelas - right.Candelas),
                left.Flags
                );
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="Units"/>.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Units(ulong flags) => new Units(flags);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Units"/> to <see cref="ulong"/>.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ulong(Units unit) => unit._id;
    }
}
