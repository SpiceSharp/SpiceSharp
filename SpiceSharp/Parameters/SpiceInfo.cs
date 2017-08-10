using System;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// This attribute allows giving descriptions for parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class SpiceInfo : System.Attribute
    {
        /// <summary>
        /// Get the description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Is this parameter interesting?
        /// </summary>
        public bool Interesting { get; set; } = true;

        /// <summary>
        /// Is this parameter a principal design parameter?
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
