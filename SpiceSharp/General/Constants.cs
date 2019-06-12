namespace SpiceSharp
{
    /// <summary>
    /// Helpful electronics-related constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Charge of an electron (C)
        /// </summary>
        public const double Charge = 1.6021918e-19;

        /// <summary>
        /// The conversion constant for converting between Kelvin and Celsius.
        /// </summary>
        public const double CelsiusKelvin = 273.15;

        /// <summary>
        /// Boltzman constant (J/K)
        /// </summary>
        public const double Boltzmann = 1.3806226e-23;

        /// <summary>
        /// The default reference temperature in Kelvin (27degC)
        /// </summary>
        public const double ReferenceTemperature = 300.15;

        /// <summary>
        /// The square root of 2
        /// </summary>
        public const double Root2 = 1.4142135623730951;

        /// <summary>
        /// The thermal voltage at the default reference temperature (V)
        /// </summary>
        public const double Vt0 = Boltzmann * (27.0 + CelsiusKelvin) / Charge;

        /// <summary>
        /// Normalized thermal voltage (V/K)
        /// </summary>
        public const double KOverQ = Boltzmann / Charge;

    }
}
