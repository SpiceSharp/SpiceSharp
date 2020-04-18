using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that a class has parameters with attributes that could have an effect
    /// on their behavior.
    /// </summary>
    /// <remarks>
    /// This attribute is used by the SpiceSharp.CodeGeneration project to find out if 
    /// a class needs to be updated.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedParametersAttribute : Attribute
    {
    }
}
