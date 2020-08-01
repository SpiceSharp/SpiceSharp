using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// Describes a behavior that can fetch behaviors after a local simulation has executed.
    /// </summary>
    public interface IParallelBehavior : IBehavior
    {
        /// <summary>
        /// Fetches the behaviors.
        /// </summary>
        /// <param name="context">The context.</param>
        void FetchBehaviors(ParallelBindingContext context);
    }
}
