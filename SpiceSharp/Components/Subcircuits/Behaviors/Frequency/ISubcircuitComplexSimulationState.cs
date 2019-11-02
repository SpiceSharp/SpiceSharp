using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IComplexSimulationState"/> for using in <see cref="FrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="IComplexSimulationState" />
    public interface ISubcircuitComplexSimulationState : IComplexSimulationState
    {
        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        void Reset();

        /// <summary>
        /// Notifies the state that these variables are shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        void ShareVariables(HashSet<Variable> common);

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        /// <returns>True if the application was succesful.</returns>
        bool ApplyAsynchronously();

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        void ApplySynchronously();

        /// <summary>
        /// Updates the behavior with the new solution.
        /// </summary>
        void Update();
    }
}
