using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that two pins are connected by a finite impedance at DC. This attribute can be
    /// applied to a <see cref="Components.Component"/> to check for floating nodes.
    /// If this attribute is not applied, then all pins are assumed to be connected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConnectedAttribute : Attribute
    {
        /// <summary>
        /// First connected pin
        /// </summary>
        public int Pin1 { get; }

        /// <summary>
        /// Second connected pin
        /// </summary>
        public int Pin2 { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pin1">First pin</param>
        /// <param name="pin2">Second pin</param>
        public ConnectedAttribute(int pin1, int pin2)
        {
            Pin1 = pin1;
            Pin2 = pin2;
        }

        /// <summary>
        /// Constructor
        /// No parameters mean no pins are connected
        /// </summary>
        public ConnectedAttribute()
        {
            Pin1 = -1;
            Pin2 = -1;
        }
    }
}
