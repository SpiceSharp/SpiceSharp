using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Specifies the name for a member. It can be applied to properties or method. Multiple names are allowed.
    /// This attribute is used to find members using reflection.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ParameterNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public ParameterNameAttribute(string name)
        {
            Name = name;
        }
    }
}
