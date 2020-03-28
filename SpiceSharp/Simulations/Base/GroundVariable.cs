using System;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A ground variable that always returns the reference.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IVariable{T}" />
    public class GroundVariable<T> : IVariable<T> where T : IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets the value of the variable.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value => default;

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name => Constants.Ground;

        /// <summary>
        /// Gets the units of the quantity.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public IUnit Unit => Units.Volt;
    }
}
