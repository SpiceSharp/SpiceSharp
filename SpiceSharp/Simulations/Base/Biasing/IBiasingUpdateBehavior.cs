using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A <see cref="IBiasingBehavior"/> that can update after solving an iteration of a <see cref="IBiasingSimulation"/>.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface IBiasingUpdateBehavior : IBehavior
    {
        /// <summary>
        /// Updates the behavior with the new solution.
        /// </summary>
        void Update();
    }
}
