using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// This attribute specifies a description and other metadata of a parameter. It can be applied to a field, property or method
    /// </summary>
    /// <remarks>
    /// If this attribute is used on a private field, the source generator will automatically generate a property.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ParameterInfoAttribute : Attribute
    {
        /// <summary>
        /// Gets the parameter description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets whether or not this parameter is interesting. Defaults to true.
        /// </summary>
        public bool Interesting { get; set; } = true;

        /// <summary>
        /// Gets or sets whether or not this parameter is a principal design parameter. Defaults to false.
        /// </summary>
        public bool IsPrincipal { get; set; } = false;

        /// <summary>
        /// Gets or sets the units of the parameter.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public string Units { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterInfoAttribute"/> class.
        /// </summary>
        /// <param name="description">The description of the parameter.</param>
        public ParameterInfoAttribute(string description)
        {
            Description = description;
        }
    }
}
