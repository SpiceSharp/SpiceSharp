using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// Describes a behavior that can be used for subcircuits.
    /// </summary>
    public interface ISubcircuitBehavior : IBehavior
    {
        /// <summary>
        /// Makes the behavior fetch the behaviors using the binding context.
        /// </summary>
        void FetchBehaviors(SubcircuitBindingContext context);
    }
}
