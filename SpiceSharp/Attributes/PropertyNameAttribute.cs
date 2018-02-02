using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Specifies the name for a member.
    /// It can be applied to properties or method. Multiple names are allowed.
    /// This is used to find members using reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class PropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        public PropertyNameAttribute(string name)
        {
            Name = name;
        }
    }
}
