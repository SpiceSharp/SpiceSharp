using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that the class contains parameters for which a set method should be implemented by the source generator.
    /// </summary>
    /// <remarks>
    /// This attribute is used by the SpiceSharpGenerator project to find out if 
    /// a class needs to be generated.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class GeneratedParametersAttribute : Attribute
    {
    }
}
