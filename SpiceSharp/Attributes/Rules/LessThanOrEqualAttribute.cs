namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that indicates a minimum value for a parameter.
    /// </summary>
    /// <seealso cref="RuleAttribute" />
    public sealed class LessThanOrEqualAttribute : RuleAttribute
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
        public LessThanOrEqualAttribute(double value)
        {
            Maximum = value;
        }
    }
}
