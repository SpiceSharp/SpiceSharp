using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class defines some standard units.
    /// </summary>
    public static class Units
    {
        /// <summary>
        /// Volt.
        /// </summary>
        public static readonly IUnit Volt = new SIUnitDefinition("V", new SIUnits(-3, 2, 1, -1, 0, 0, 0));

        /// <summary>
        /// Ampere.
        /// </summary>
        public static readonly IUnit Ampere = new SIUnitDefinition("A", new SIUnits(0, 0, 0, 1, 0, 0, 0));

        /// <summary>
        /// Coulomb.
        /// </summary>
        public static readonly IUnit Coulomb = new SIUnitDefinition("C", new SIUnits(1, 0, 0, 1, 0, 0, 0));

        /// <summary>
        /// Ohm.
        /// </summary>
        public static readonly IUnit Ohm = new SIUnitDefinition("\u03a9", new SIUnits(-3, 2, 1, 0, 0, 0, 0));

        /// <summary>
        /// Seconds.
        /// </summary>
        public static readonly IUnit Seconds = new SIUnitDefinition("s", new SIUnits(1, 0, 0, 0, 0, 0, 0));
    }
}
