using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// Indicates that a property is derived from other properties.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DerivedPropertyAttribute : Attribute
    {
    }
}
