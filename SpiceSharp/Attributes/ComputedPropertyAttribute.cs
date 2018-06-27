using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that a property is computed based on other properties or sets other properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ComputedPropertyAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ComputedPropertyAttribute()
        {
        }
    }
}
