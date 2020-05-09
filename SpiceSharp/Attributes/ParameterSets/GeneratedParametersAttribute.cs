using System;

namespace SpiceSharp.ParameterSets
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
        /// <summary>
        /// Gets or sets a value indicating whether the class is extended with methods for setting or getting
        /// the parameters by their name by the code generator.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the the class should be extended with methods for setting or getting parameters; otherwise, <c>false</c>.
        /// </value>
        public bool AddNames { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the rule attributes should be made explicit in the getters and setters
        /// of properties.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the rule attributes should be made explicit in the getters and setters; otherwise, <c>false</c>.
        /// </value>
        public bool AddRules { get; set; } = true;
    }
}
