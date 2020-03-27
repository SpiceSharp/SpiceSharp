using System;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// The most straight-forward implementation of a unit: one that is directly
    /// derived from SI units.
    /// </summary>
    /// <seealso cref="IUnit" />
    public class SIUnitDefinition : IUnit
    {
        private readonly string _name;

        /// <summary>
        /// Gets the base value expressed in SI units (no transformations applied).
        /// </summary>
        /// <value>
        /// The base value.
        /// </value>
        public double BaseValue => 1.0;

        /// <summary>
        /// Gets the SI units.
        /// </summary>
        /// <value>
        /// The SI units.
        /// </value>
        public SIUnits SI { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SIUnitDefinition"/> class.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="units">The SI units.</param>
        public SIUnitDefinition(string name, SIUnits units)
        {
            _name = name.ThrowIfNull(nameof(name));
            SI = units;
        }

        /// <summary>
        /// Converts a base value expressed strictly in SI units (no transformations) to this unit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The value in these units.
        /// </returns>
        public double From(double value) => value;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IUnit other)
        {
            if (other == this)
                return true;
            if (SI != other.SI)
                return false;
            if (!other.BaseValue.Equals(1.0))
                return false;
            return true;
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
            if (obj is IUnit unit)
                return ((IEquatable<IUnit>)this).Equals(unit);
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => SI.GetHashCode() ^ 1.0.GetHashCode();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => _name;
    }
}
