using SpiceSharp.Simulations.Variables;
using System;

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

        /// <inheritdoc/>
        public double BaseValue => 1.0;

        /// <inheritdoc/>
        public SIUnits SI { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SIUnitDefinition"/> class.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="units">The SI units.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public SIUnitDefinition(string name, SIUnits units)
        {
            _name = name.ThrowIfNull(nameof(name));
            SI = units;
        }

        /// <inheritdoc/>
        public double From(double value) => value;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
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
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
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
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => _name;
    }
}
