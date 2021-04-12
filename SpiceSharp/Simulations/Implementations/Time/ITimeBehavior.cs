using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface for time-dependent behaviors.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface ITimeBehavior : IBehavior
    {
        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="IDerivative" /> or <see cref="IIntegrationState" />.
        /// </remarks>
        void InitializeStates();
    }
}
