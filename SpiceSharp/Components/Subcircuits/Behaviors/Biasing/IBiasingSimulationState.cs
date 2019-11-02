using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingSimulationState"/> for using in <see cref="BiasingBehavior"/>.
    /// </summary>
    public interface ISubcircuitBiasingSimulationState : IBiasingSimulationState
    {
        /// <summary>
        /// Notifies the state that these variables are shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        void ShareVariables(HashSet<Variable> common);

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        void Reset();

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        bool CheckConvergence();

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        /// <returns>True if the application was succesful.</returns>
        bool ApplyAsynchronously();

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        void ApplySynchronously();
    }
}
