using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Indicates that the circuit component is an independent source. This attribute can
    /// be applied to an <see cref="ICircuitComponent"/> to check for the existance of at
    /// least one independent source.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndependentSource : Attribute
    {
    }
}
