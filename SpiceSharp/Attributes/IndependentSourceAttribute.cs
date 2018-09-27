using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that the circuit component is an independent source. This attribute can
    /// be applied to a <see cref="Components.Component" /> to check for the existence of at
    /// least one independent source.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class IndependentSourceAttribute : Attribute
    {
    }
}
