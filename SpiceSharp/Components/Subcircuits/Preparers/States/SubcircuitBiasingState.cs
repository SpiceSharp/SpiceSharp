using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingSimulationState"/> for using in <see cref="BiasingBehavior"/>.
    /// </summary>
    public abstract class SubcircuitBiasingState
    {
        /// <summary>
        /// Notifies the state that these variables can be shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public abstract void ShareVariables(HashSet<Variable> common);

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CheckConvergence();

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        public abstract void ApplyAsynchroneously();

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public abstract void ApplySynchroneously();
    }
}
