using System;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// This attribute allows giving (multiple) names
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class SpiceName : Attribute
    {
        /// <summary>
        /// Get the name of the parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public SpiceName(string name)
        {
            Name = name;
        }
    }
}
