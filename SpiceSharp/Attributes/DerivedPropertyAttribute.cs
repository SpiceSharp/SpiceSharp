using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that a property is derived from other properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DerivedPropertyAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DerivedPropertyAttribute()
        {
        }
    }
}
