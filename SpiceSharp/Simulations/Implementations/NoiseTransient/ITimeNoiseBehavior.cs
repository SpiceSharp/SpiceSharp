using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors;

/// <summary>
/// A behavior that is used by <see cref="NoiseTransient"/>.
/// </summary>
/// <seealso cref="ITimeNoiseSource"/>
/// <seealso cref="IBiasingBehavior" />
[SimulationBehavior]
public interface ITimeNoiseBehavior : ITimeNoiseSource, IBiasingBehavior
{
    /// <summary>
    /// Refreshes the noise densities from the last accepted operating point, and advances the
    /// shaping state of every noise source of the device over the probed timestep.
    /// </summary>
    void ProbeNoise();
}
