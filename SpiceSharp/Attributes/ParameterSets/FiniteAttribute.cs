using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that indicates a finite value for a parameter. This means the value should not be NaN or infinity.
    /// </summary>
    /// <remarks>
    /// If this attribute is used on a private field, the source generator will automatically generate a property.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class FiniteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanAttribute"/> class.
        /// </summary>
        public FiniteAttribute()
        {
        }
    }
}
