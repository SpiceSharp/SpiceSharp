using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Indicates that pins are connected by a finite impedance at DC. This attribute can be
    /// applied to a <see cref="Component"/> to check for floating nodes.
    /// If multiple attributes are specified, they are treated as separately connected pins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConnectedAttribute : Attribute
    {
        /// <summary>
        /// Gets the connected pins
        /// </summary>
        public int[] Pins { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pins">The internally connected pins</param>
        public ConnectedAttribute(params int[] pins)
        {
            Pins = pins;
        }
    }
}
