using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that indicates a minimum limit for a parameter value.
    /// </summary>
    /// <remarks>
    /// If this attribute is used on a private field, the source generator will automatically generate a property.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class LowerLimitAttribute : Attribute
    {
        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double Minimum { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LowerLimitAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public LowerLimitAttribute(double value)
        {
            Minimum = value;
        }
    }
}
