using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Simulations
{
    public partial struct Units
    {
        private const int _sOffset = 0;
        private const int _mOffset = 8;
        private const int _kgOffset = 16;
        private const int _aOffset = 24;
        private const int _kOffset = 32;
        private const int _molOffset = 40;
        private const int _cdOffset = 48;
        private const int _flagOffset = 56;

        /// <summary>
        /// Flags when using alternate units.
        /// </summary>
        [Flags]
        public enum Alternate : byte
        {
            /// <summary>
            /// No alternate units.
            /// </summary>
            None = 0x00,

            /// <summary>
            /// Alternate units of temperature (Celsius).
            /// </summary>
            Celsius = 0x01,
        }

        /// <summary>
        /// Display names.
        /// </summary>
        public static readonly Dictionary<Units, string> Display = new Dictionary<Units, string>
        {
            { 0x0000000000000001UL, "s" },
            { 0x0000000000000100UL, "m" },
            { 0x0000000000010000UL, "kg" },
            { 0x0000000001000000UL, "A" },
            { 0x0000000100000000UL, "K" },
            { 0x0100000100000000UL, "\u00b0C" },
            { 0x0000010000000000UL, "mol" },
            { 0x0001000000000000UL, "cd" },
            { 0x00000000000000ffUL, "Hz" },
            { 0x00000000ff0102fdUL, "V" },
            { 0x00000000ff0101fdUL, "V/m" },
            { 0x0000000001000001UL, "C" },
            { 0x00000000000102fdUL, "W" },
            { 0x00000000fe0102fdUL, "\u2126" },
            { 0x000000fffe0102fdUL, "\u2126/K" },
            { 0x000000fefe0102fdUL, "\u2126/K^2" },
            { 0x0000000002fffe03UL, "S" },
            { 0x0000000002fffe04UL, "F" },
            { 0x0000000002fffd04UL, "F/m" },
            { 0x0000000002fffc04UL, "F/m^2" },
            { 0x000000ff02fffe04UL, "F/K" },
            { 0x000000fe02fffe04UL, "F/K^2" },
            { 0x00000000fe0102feUL, "H" },
        };
        
        /// <summary>
        /// Time (seconds).
        /// </summary>
        public const ulong Second = 0x0000000000000001UL;
    
        /// <summary>
        /// Distance (meters).
        /// </summary>
        public const ulong Meter = 0x0000000000000100UL;
    
        /// <summary>
        /// Weight (kilograms).
        /// </summary>
        public const ulong Kilogram = 0x0000000000010000UL;
    
        /// <summary>
        /// Current (Ampere).
        /// </summary>
        public const ulong Ampere = 0x0000000001000000UL;
    
        /// <summary>
        /// Temperature (Kelvin).
        /// </summary>
        public const ulong Kelvin = 0x0000000100000000UL;
    
        /// <summary>
        /// Temperature (Celsius).
        /// </summary>
        public const ulong Celsius = 0x0100000100000000UL;
    
        /// <summary>
        /// Amount of substance (mole).
        /// </summary>
        public const ulong Mole = 0x0000010000000000UL;
    
        /// <summary>
        /// Luminous intensity (Candela).
        /// </summary>
        public const ulong Candela = 0x0001000000000000UL;
    
        /// <summary>
        /// Frequency (Hertz).
        /// </summary>
        public const ulong Hertz = 0x00000000000000ffUL;
    
        /// <summary>
        /// Voltage (Volts).
        /// </summary>
        public const ulong Volt = 0x00000000ff0102fdUL;
    
        /// <summary>
        /// Electric field (Volts per meter).
        /// </summary>
        public const ulong VoltPerMeter = 0x00000000ff0101fdUL;
    
        /// <summary>
        /// Charge (Coulombs).
        /// </summary>
        public const ulong Coulomb = 0x0000000001000001UL;
    
        /// <summary>
        /// Power (Watts).
        /// </summary>
        public const ulong Watt = 0x00000000000102fdUL;
    
        /// <summary>
        /// Resistance (Ohms).
        /// </summary>
        public const ulong Ohm = 0x00000000fe0102fdUL;
    
        /// <summary>
        /// Resistance per Kelvin (Ohms per Kelvin).
        /// </summary>
        public const ulong OhmPerKelvin = 0x000000fffe0102fdUL;
    
        /// <summary>
        /// Resistance per squared Kelvin (Ohms per squared Kelvin).
        /// </summary>
        public const ulong OhmPerKelvin2 = 0x000000fefe0102fdUL;
    
        /// <summary>
        /// Conductance (Mho/Siemens).
        /// </summary>
        public const ulong Mho = 0x0000000002fffe03UL;
    
        /// <summary>
        /// Capacitance (Farads).
        /// </summary>
        public const ulong Farad = 0x0000000002fffe04UL;
    
        /// <summary>
        /// Capacitance per meter (Farads per meter).
        /// </summary>
        public const ulong FaradPerMeter = 0x0000000002fffd04UL;
    
        /// <summary>
        /// Capacitance per area (Farads per squared meter).
        /// </summary>
        public const ulong FaradPerArea = 0x0000000002fffc04UL;
    
        /// <summary>
        /// Capacitance per Kelvin (Farads per Kelvin).
        /// </summary>
        public const ulong FaradPerKelvin = 0x000000ff02fffe04UL;
    
        /// <summary>
        /// Capacitance per squared Kelvin (Farads per squared Kelvin).
        /// </summary>
        public const ulong FaradPerKelvin2 = 0x000000fe02fffe04UL;
    
        /// <summary>
        /// Inductance (Henries).
        /// </summary>
        public const ulong Henry = 0x00000000fe0102feUL;
    
    }
}
