using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that indicates a minimum value for a parameter.
    /// </summary>
    /// <remarks>
    /// If this attribute is used on a private field, the source generator will automatically generate a property.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class LessThanOrEqualsAttribute : Attribute
    {
        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double Maximum { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualsAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public LessThanOrEqualsAttribute(double value)
        {
            Maximum = value;
        }
    }
}
