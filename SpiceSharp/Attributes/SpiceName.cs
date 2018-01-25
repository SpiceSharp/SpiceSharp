using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Specifies the name for a parameter of a class that extends <see cref="Parameterized{T}"/>.
    /// It can be applied to a field, property or method. Multiple names are allowed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class NameAttribute : Attribute
    {
        /// <summary>
        /// Get the name of the parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
