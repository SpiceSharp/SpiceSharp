using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that describes a rule.
    /// </summary>
    /// <remarks>
    /// Rule attributes can be converted to code automatically using SpiceSharp.CodeGeneration.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class RuleAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the rule may raise an exception.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the rule may raise an exception; otherwise, <c>false</c>.
        /// </value>
        public bool RaisesException { get; set; }
    }
}
