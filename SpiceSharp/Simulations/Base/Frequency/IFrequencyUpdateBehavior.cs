using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior"/> that can update after solving an iteration of a <see cref="FrequencySimulation"/>.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface IFrequencyUpdateBehavior : IBehavior
    {
        /// <summary>
        /// Updates the behavior with the new solution.
        /// </summary>
        void Update();
    }
}
