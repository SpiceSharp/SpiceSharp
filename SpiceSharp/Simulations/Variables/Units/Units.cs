using System;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class defines the standard units.
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

        private class SIUnitDefinition : IUnit
        {
            private readonly string _name;
            private readonly SIUnits _si;
            double IUnit.BaseValue => 1.0;
            SIUnits IUnit.SI => _si;
            public SIUnitDefinition(string name, SIUnits units)
            {
                _name = name.ThrowIfNull(nameof(name));
                _si = units;
            }
            double IUnit.From(double value) => value;
            bool IEquatable<IUnit>.Equals(IUnit other)
            {
                if (other == this)
                    return true;
                if (_si != other.SI)
                    return false;
                if (!other.BaseValue.Equals(1.0))
                    return false;
                return true;
            }
            public override bool Equals(object obj)
            {
                if (obj is IUnit unit)
                    return ((IEquatable<IUnit>)this).Equals(unit);
                return false;
            }
            public override int GetHashCode() => _si.GetHashCode() ^ 1.0.GetHashCode(); // Just for uniformity
            public override string ToString() => _name;
        }
    }
}
