using System;

namespace SpiceSharp.Simulations.Variables
{
    /// <summary>
    /// Describes a (possibly derived) unit that can be expressed in some way in terms of SI units.
    /// </summary>
    public interface IUnit : IEquatable<IUnit>
    {
        /// <summary>
        /// Gets the base value expressed in SI units (no transformations applied).
        /// </summary>
        /// <value>
        /// The base value.
        /// </value>
        double BaseValue { get; }

        /// <summary>
        /// Gets the SI units.
        /// </summary>
        /// <value>
        /// The SI units.
        /// </value>
        SIUnits SI { get; }

        /// <summary>
        /// Converts a base value expressed strictly in SI units (no transformations) to this unit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The value in these units.
        /// </returns>
        double From(double value);
    }
}
