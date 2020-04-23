using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An interface describing a behavior with a branch equation.
    /// </summary>
    public interface IBranchedBehavior<T> : IBehavior
    {
        /// <summary>
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        IVariable<T> Branch { get; }
    }
}
