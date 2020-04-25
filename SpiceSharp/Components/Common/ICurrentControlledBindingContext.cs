using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// An <see cref="IComponentBindingContext"/> for a current-controlled component.
    /// </summary>
    /// <seealso cref="IComponentBindingContext" />
    public interface ICurrentControlledBindingContext : IComponentBindingContext
    {
        /// <summary>
        /// Gets the behaviors of the controlling source.
        /// </summary>
        /// <value>
        /// The behaviors of the controlling source, or <c>null</c> if it wasn't found.
        /// </value>
        IBehaviorContainer ControlBehaviors { get; }
    }
}
