using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Describes a binding context that is created to work for a specified entity type.
    /// This attribute is used to specify for which entity the binding context is created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class BindingContextForAttribute : Attribute
    {
        /// <summary>
        /// Gets the entity type that the behavior implementation is targeting.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the generic type arguments that should apply for this behavior when used for the specified entity.
        /// </summary>
        /// <value>
        /// The generic type arguments.
        /// </value>
        public Type[] GenericTypeArguments { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorForAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity that the behavior is meant for.</param>
        /// <param name="genericTypeArguments">The generic type arguments if needed.</param>
        public BindingContextForAttribute(Type entityType, Type[] genericTypeArguments)
        {
            EntityType = entityType;
            GenericTypeArguments = genericTypeArguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorForAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity that the behavior is meant for.</param>
        public BindingContextForAttribute(Type entityType)
        {
            EntityType = entityType;
            GenericTypeArguments = null;
        }
    }
}
