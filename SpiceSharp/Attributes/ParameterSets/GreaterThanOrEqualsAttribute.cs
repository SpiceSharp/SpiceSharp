using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An attribute that indicates a minimum value for a parameter.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class GreaterThanOrEqualsAttribute : Attribute
    {
        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double Minimum { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterThanAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public GreaterThanOrEqualsAttribute(double value)
        {
            Minimum = value;
        }
    }
}
