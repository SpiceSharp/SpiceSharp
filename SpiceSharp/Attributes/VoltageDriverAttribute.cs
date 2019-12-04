using SpiceSharp.Components;
using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that two nodes are driven by a voltage source. This attribute can
    /// be applied to a <see cref="Component" /> to check for voltage loops.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class VoltageDriverAttribute : Attribute
    {
        /// <summary>
        /// The pin connected to the positive side of the voltage source.
        /// </summary>
        public int Positive { get; }

        /// <summary>
        /// The pin connected to the negative side of the voltage source.
        /// </summary>
        public int Negative { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDriverAttribute"/> class.
        /// </summary>
        /// <param name="positive">The positive pin of the source.</param>
        /// <param name="negative">The negative pin of the source.</param>
        public VoltageDriverAttribute(int positive, int negative)
        {
            Positive = positive;
            Negative = negative;
        }
    }
}
