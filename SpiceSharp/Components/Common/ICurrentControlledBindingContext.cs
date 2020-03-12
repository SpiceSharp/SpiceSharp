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
        /// The control behaviors.
        /// </value>
        IBehaviorContainer ControlBehaviors { get; }
    }
}
