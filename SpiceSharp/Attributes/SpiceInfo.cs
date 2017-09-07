using System;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// Specifies a description and other metadata of a parameter for a class that extends <see cref="Parameterized{T}"/>.
    /// It can be applied to a field, property or method
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class SpiceInfo : Attribute
    {
        /// <summary>
        /// Get the parameter description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Is this parameter interesting? Defaults to true.
        /// </summary>
        public bool Interesting { get; set; } = true;

        /// <summary>
        /// Is this parameter a principal design parameter? Defaults to false.
        /// </summary>
        public bool IsPrincipal { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description"></param>
        public SpiceInfo(string description)
        {
            Description = description;
        }
    }
}
