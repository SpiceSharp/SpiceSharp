using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Transient behavior
    /// </summary>
    public abstract class TransientBehavior : Behavior
    {
        /// <summary>
        /// Transient calculations
        /// </summary>
        /// <param name="sim">Time-base simulation</param>
        public abstract void Transient(TimeSimulation sim);
    }
}
