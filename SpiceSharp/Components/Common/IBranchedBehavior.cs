using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An interface describing a behavior with a branch equation.
    /// </summary>
    public interface IBranchedBehavior : IBehavior
    {
        /// <summary>
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        IVariable Branch { get; }
    }
}
