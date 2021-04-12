using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Indicates that a behavior needs another behavior in order to work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class BehaviorRequiresAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the required behavior.
        /// </summary>
        /// <value>
        /// The required behavior type.
        /// </value>
        public Type RequiredBehavior { get; }

        /// <summary>
        /// Gets the generic parameters of the behavior.
        /// </summary>
        public Type[] GenericTypeParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorRequiresAttribute"/> class.
        /// </summary>
        /// <param name="required">The required type.</param>
        public BehaviorRequiresAttribute(Type required)
        {
            RequiredBehavior = required;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorRequiresAttribute"/> class.
        /// </summary>
        /// <param name="required">The required type.</param>
        /// <param name="genericTypeParameters">The generic parameters.</param>
        public BehaviorRequiresAttribute(Type required, Type[] genericTypeParameters)
        {
            RequiredBehavior = required;
            GenericTypeParameters = genericTypeParameters;
        }
    }
}
