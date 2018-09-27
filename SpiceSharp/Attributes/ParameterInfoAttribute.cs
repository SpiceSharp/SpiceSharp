using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// This attribute specifies a description and other metadata of a parameter. It can be applied to a field, property or method
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
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
        /// Initializes a new instance of the <see cref="ParameterInfoAttribute"/> class.
        /// </summary>
        /// <param name="description">The description of the parameter.</param>
        public ParameterInfoAttribute(string description)
        {
            Description = description;
        }
    }
}
