using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that two nodes are driven by a voltage source. This attribute can
    /// be applied to a <see cref="Components.Component"/> to check for voltage loops.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class VoltageDriverAttribute : Attribute
    {
        /// <summary>
        /// The pin connected to the positive side of the voltage source
        /// </summary>
        public int Positive
        {
            get
            {
                return pos;
            }
        }

        /// <summary>
        /// The pin connected to the negative side of the voltage source
        /// </summary>
        public int Negative
        {
            get
            {
                return neg;
            }
        }

        /// <summary>
        /// Pins that drive the voltage
        /// </summary>
        int pos, neg;

        /// <summary>
        /// Constructor
        /// This attribute specifies that two nodes are connected by a voltage source. It can
        /// be used for checking voltage loops.
        /// </summary>
        /// <param name="pos">The positive output pin index</param>
        /// <param name="neg">The negative output pin index</param>
        public VoltageDriverAttribute(int pos, int neg)
        {
            this.pos = pos;
            this.neg = neg;
        }
    }
}
