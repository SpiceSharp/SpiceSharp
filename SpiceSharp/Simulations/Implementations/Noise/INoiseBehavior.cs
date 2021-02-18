using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A behavior that is used by <see cref="Noise" />.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface INoiseBehavior : INoiseSource, IBehavior
    {
        /// <summary>
        /// Computes the noise contributions.
        /// </summary>
        void Compute();
    }
}
