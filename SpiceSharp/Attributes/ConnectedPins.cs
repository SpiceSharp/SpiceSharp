using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Indicates that pins are connected by a finite impedance at DC. This attribute can be
    /// applied to an <see cref="ICircuitComponent"/> to check for floating nodes.
    /// If multiple attributes are specified, they are treated as separately connected pins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConnectedPins : Attribute
    {
        /// <summary>
        /// Gets the connected pins
        /// </summary>
        public int[] Pins { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pins">The internally connected pins</param>
        public ConnectedPins(params int[] pins)
        {
            Pins = pins;
        }
    }
}
