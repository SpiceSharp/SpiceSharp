using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that pins are connected by a finite impedance at DC. This attribute can be
    /// applied to a <see cref="Components.Component"/> to check for floating nodes.
    /// If multiple attributes are specified, they are treated as separately connected pins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConnectedAttribute : Attribute
    {
        /// <summary>
        /// Get a connect pin
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public int this[int index]
        {
            get => pins[index];
        }

        /// <summary>
        /// Gets the number of pins
        /// </summary>
        public int Count { get => pins.Length; }

        /// <summary>
        /// Private variables
        /// </summary>
        int[] pins;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pins">The internally connected pins</param>
        public ConnectedAttribute(params int[] pins)
        {
            this.pins = pins;
        }
    }
}
