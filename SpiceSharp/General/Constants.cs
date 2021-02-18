using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Helpful (electronics-related) constants used throughout Spice#.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Charge of an electron (in Coulomb - C).
        /// </summary>
        public const double Charge = 1.6021918e-19;

        /// <summary>
        /// The conversion constant for converting between Kelvin and Celsius (in Kelvin or degrees Celsius).
        /// </summary>
        public const double CelsiusKelvin = 273.15;

        /// <summary>
        /// Boltzman constant (in Joules per Kelvin - J/K).
        /// </summary>
        public const double Boltzmann = 1.3806226e-23;

        /// <summary>
        /// The default reference temperature in Kelvin (equal to 27 degrees Celsius).
        /// </summary>
        public const double ReferenceTemperature = 300.15;

        /// <summary>
        /// The square root of 2.
        /// </summary>
        public const double Root2 = 1.4142135623730951;

        /// <summary>
        /// The thermal voltage at the default reference temperature (in Volt - V).
        /// </summary>
        public const double Vt0 = Boltzmann * (27.0 + CelsiusKelvin) / Charge;

        /// <summary>
        /// Normalized thermal voltage (in Volts per Kelvin - V/K).
        /// </summary>
        public const double KOverQ = Boltzmann / Charge;

        /// <summary>
        /// The fixed name of the ground node.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The ground node is the reference node at 0V, and any simulation that
        /// wants to solve for voltages will need one.
        /// </para>
        /// <para>
        /// If you want to use other names for ground, you can make an
        /// <see cref="System.Collections.Generic.IEqualityComparer{T}" /> that
        /// maps these extra node names to the ground node.
        /// </para>
        /// </remarks>
        public const string Ground = "0";

        /// <summary>
        /// Gets the default string comparer.
        /// </summary>
        /// <value>
        /// The default string comparer.
        /// </value>
        public static IEqualityComparer<string> DefaultComparer { get; } = StringComparer.OrdinalIgnoreCase;
    }
}
