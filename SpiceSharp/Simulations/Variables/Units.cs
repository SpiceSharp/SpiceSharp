using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes the units of a variable. It is expressed in SI units.
    /// The struct is interchangable with a <see cref="ulong"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Units
    {
        private const int _sOffset = 0;
        private const int _mOffset = 8;
        private const int _kgOffset = 16;
        private const int _aOffset = 24;
        private const int _kOffset = 32;
        private const int _molOffset = 40;
        private const int _cdOffset = 48;

        /// <summary>
        /// Voltage (Volt).
        /// </summary>
        public const ulong Volt = (0x01UL << _kgOffset) | (0x02UL << _mOffset) | (0xfdUL << _sOffset) | (0xffUL << _aOffset);

        /// <summary>
        /// Electric field (Volt per meter).
        /// </summary>
        public const ulong VoltPerMeter = (0x01UL << _kgOffset) | (0x01UL << _mOffset) | (0xfdUL << _sOffset) | (0xffUL << _aOffset);

        /// <summary>
        /// Current (Ampere).
        /// </summary>
        public const ulong Ampere = 0x01UL << _aOffset;

        /// <summary>
        /// Resistance (Ohm).
        /// </summary>
        public const ulong Ohm = (0x01UL << _kgOffset) | (0x02UL << _mOffset) | (0xfdUL << _sOffset) | (0xfeUL << _aOffset);

        /// <summary>
        /// Conductance (Mho).
        /// </summary>
        public const ulong Mho = (0xffUL << _kgOffset) | (0xfeUL << _mOffset) | (0x03UL << _sOffset) | (0x02UL << _aOffset);

        /// <summary>
        /// Inductance (Henry).
        /// </summary>
        public const ulong Henry = (0x01UL << _kgOffset) | (0x02UL << _mOffset) | (0xfeUL << _sOffset) | (0xfeUL << _aOffset);

        /// <summary>
        /// Capacitance (Farad).
        /// </summary>
        public const ulong Farad = (0xffUL << _kgOffset) | (0xfeUL << _mOffset) | (0x04UL << _sOffset) | (0x02UL << _aOffset);

        /// <summary>
        /// Power (Watt).
        /// </summary>
        public const ulong Watt = (0x01UL << _kgOffset) | (0x02UL << _mOffset) | (0xfd << _sOffset);

        /// <summary>
        /// Frequency (Hertz).
        /// </summary>
        public const ulong Hertz = (0xffUL << _sOffset);

        /// <summary>
        /// Time (Second).
        /// </summary>
        public const ulong Second = 0x01UL << _sOffset;

        /// <summary>
        /// Distance (Meter).
        /// </summary>
        public const ulong Meter = 0x01UL << _mOffset;

        /// <summary>
        /// Weight (Kilogram).
        /// </summary>
        public const ulong Kilogram = 0x01UL << _kgOffset;

        /// <summary>
        /// Temperature (Kelvin).
        /// </summary>
        public const ulong Kelvin = 0x01UL << _kOffset;

        /// <summary>
        /// Amount of substance (Mole).
        /// </summary>
        public const ulong Mole = 0x01UL << _molOffset;

        /// <summary>
        /// Luminous intensity (Candela).
        /// </summary>
        public const ulong Candela = 0x01UL << _cdOffset;

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
        /// Initializes a new instance of the <see cref="Units"/> struct.
        /// </summary>
        /// <param name="s">The exponent for seconds.</param>
        /// <param name="m">The exponent for meters.</param>
        /// <param name="kg">The exponent for kg.</param>
        /// <param name="a">The exponent for amps.</param>
        /// <param name="k">The exponent for kelvin.</param>
        /// <param name="mol">The exponent for mol.</param>
        /// <param name="cd">The exponent for cd.</param>
        public Units(sbyte s, sbyte m, sbyte kg, sbyte a, sbyte k, sbyte mol, sbyte cd)
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
            switch (_id)
            {
                case Volt: return "V";
                case Ampere: return "A";
                case VoltPerMeter: return "V/m";
                case Ohm: return "Ohm";
                case Mho: return "Mho";
                case Watt: return "W";
                case Hertz: return "Hz";
                case Farad: return "F";
                case Henry: return "H";
                default:
                    var result = new List<string>(7);
                    if (Second != 0)
                        result.Add("s" + (Seconds != 1 ? Seconds.ToString() : ""));
                    if (Meters != 0)
                        result.Add("m" + (Meters != 1 ? Meters.ToString() : ""));
                    if (Kilograms != 0)
                        result.Add("kg" + (Kilograms != 1 ? Kilograms.ToString() : ""));
                    if (Amperes != 0)
                        result.Add("A" + (Amperes != 1 ? Amperes.ToString() : ""));
                    if (Kelvins != 0)
                        result.Add("K" + (Kelvins != 1 ? Kelvins.ToString() : ""));
                    if (Moles != 0)
                        result.Add("mol" + (Moles != 1 ? Moles.ToString() : ""));
                    if (Candelas != 0)
                        result.Add("cd" + (Candelas != 1 ? Candelas.ToString() : ""));
                    return string.Join("*", result);
            }
        }

        /// <summary>
        /// Unit multiplication.
        /// </summary>
        /// <param name="left">Left factor.</param>
        /// <param name="right">Right factor.</param>
        /// <returns>The multiplied units.</returns>
        public static Units operator *(Units left, Units right)
        {
            return new Units(
                (sbyte)(left.Seconds + right.Seconds),
                (sbyte)(left.Meters + right.Meters),
                (sbyte)(left.Kilograms + right.Kilograms),
                (sbyte)(left.Amperes + right.Amperes),
                (sbyte)(left.Kelvins + right.Kelvins),
                (sbyte)(left.Moles + right.Moles),
                (sbyte)(left.Candelas + right.Candelas)
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
            return new Units(
                (sbyte)(left.Seconds - right.Seconds),
                (sbyte)(left.Meters - right.Meters),
                (sbyte)(left.Kilograms - right.Kilograms),
                (sbyte)(left.Amperes - right.Amperes),
                (sbyte)(left.Kelvins - right.Kelvins),
                (sbyte)(left.Moles - right.Moles),
                (sbyte)(left.Candelas - right.Candelas)
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
