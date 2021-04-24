using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that a property is derived from other properties.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DerivedPropertyAttribute : Attribute
    {
    }
}
