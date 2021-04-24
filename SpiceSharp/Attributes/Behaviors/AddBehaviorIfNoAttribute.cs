using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that a behavior can be created if the specified behavior type
    /// is requested by the simulation, and if it wasn't created before.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AddBehaviorIfNoAttribute : Attribute
    {
        /// <summary>
        /// Gets the target behavior type that needs to be checked.
        /// </summary>
        /// <value>
        /// The target type.
        /// </value>
        public Type Target { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddBehaviorIfNoAttribute"/> class.
        /// </summary>
        /// <param name="target">The target behavior.</param>
        public AddBehaviorIfNoAttribute(Type target)
        {
            Target = target;
        }
    }
}
