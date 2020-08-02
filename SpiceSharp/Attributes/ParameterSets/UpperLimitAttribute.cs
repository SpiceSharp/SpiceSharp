using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An attribute that indicates a minimum limit for a parameter value.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UpperLimitAttribute : Attribute
    {
        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double Maximum { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterThanAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public UpperLimitAttribute(double value)
        {
            Maximum = value;
        }
    }
}
