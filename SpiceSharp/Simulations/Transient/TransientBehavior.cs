using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Transient behavior
    /// </summary>
    public abstract class TransientBehavior : Behavior
    {
        /// <summary>
        /// Register states
        /// </summary>
        /// <param name="pool">Pool</param>
        public virtual void RegisterStates(StatePool pool)
        {
            // Do nothing (for now)
        }

        /// <summary>
        /// Calculate the state values from the DC solution
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public virtual void GetDCstate(TimeSimulation sim)
        {
            // Do nothing (for now)
        }

        /// <summary>
        /// Transient calculations
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public abstract void Transient(TimeSimulation sim);
    }
}
