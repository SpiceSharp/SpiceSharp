using SpiceSharp.Components;
using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that two pins are connected by a finite impedance at DC. This attribute can be
    /// applied to an <see cref="Component" /> to check for floating nodes.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConnectedAttribute : Attribute
    {
        /// <summary>
        /// Gets the first connected pin index.
        /// </summary>
        public int Pin1 { get; }

        /// <summary>
        /// Gets the second connected pin index.
        /// </summary>
        public int Pin2 { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedAttribute"/> class.
        /// </summary>
        /// <param name="pin1">The first pin index.</param>
        /// <param name="pin2">The second pin index.</param>
        public ConnectedAttribute(int pin1, int pin2)
        {
            Pin1 = pin1;
            Pin2 = pin2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedAttribute"/> class.
        /// </summary>
        public ConnectedAttribute()
        {
            Pin1 = -1;
            Pin2 = -1;
        }
    }
}
