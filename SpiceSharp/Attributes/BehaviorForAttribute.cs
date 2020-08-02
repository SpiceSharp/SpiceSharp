using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Describes a behavior that is created to work for a specified entity type.
    /// This attribute is used when using dependency injection.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class BehaviorForAttribute : Attribute
    {
        /// <summary>
        /// Gets the entity type that the behavior implementation is targeting.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the behavior type that the behavior implementation is targeting.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Type Target { get; }

        /// <summary>
        /// Gets the generic type arguments.
        /// </summary>
        /// <value>
        /// The generic type arguments.
        /// </value>
        public Type[] GenericTypeArguments { get; }

        /// <summary>
        /// Gets the priority for creating the behavior.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorForAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity that the behavior is meant for.</param>
        /// <param name="target">The behavior type that the behavior is targeting.</param>
        /// <param name="genericTypeArguments">The generic type arguments if needed.</param>
        /// <param name="priority">The priority for creating this behavior.</param>
        public BehaviorForAttribute(Type entityType, Type target, Type[] genericTypeArguments, int priority)
        {
            EntityType = entityType;
            Target = target;
            Priority = 0;
            GenericTypeArguments = genericTypeArguments;
            Priority = priority;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorForAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity that the behavior is meant for.</param>
        /// <param name="target">The behavior type that the behavior is targeting.</param>
        /// <param name="genericTypeArguments">The generic type arguments if needed.</param>
        public BehaviorForAttribute(Type entityType, Type target, Type[] genericTypeArguments)
        {
            EntityType = entityType;
            Target = target;
            Priority = 0;
            GenericTypeArguments = genericTypeArguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorForAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity that the behavior is meant for.</param>
        /// <param name="target">The behavior type that the behavior is targeting.</param>
        /// <param name="priority">The priority for creating this behavior.</param>
        public BehaviorForAttribute(Type entityType, Type target, int priority)
        {
            EntityType = entityType;
            Target = target;
            Priority = priority;
            GenericTypeArguments = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorForAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity that the behavior is meant for.</param>
        /// <param name="target">The behavior type that the behavior is targeting.</param>
        public BehaviorForAttribute(Type entityType, Type target)
        {
            EntityType = entityType;
            Target = target;
            Priority = 0;
            GenericTypeArguments = null;
        }
    }
}
