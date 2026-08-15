using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors;

/// <summary>
/// A behavior that is used by <see cref="NoiseTransient"/>.
/// </summary>
/// <seealso cref="ITimeNoiseSource"/>
/// <seealso cref="IBehavior" />
[SimulationBehavior]
public interface ITimeNoiseBehavior : ITimeNoiseSource, IBehavior
{
    /// <summary>
    /// Refreshes the noise densities from the last accepted operating point, and advances the
    /// shaping state of every noise source of the device over the probed timestep.
    /// </summary>
    /// <remarks>
    /// Called once per probed timepoint, never inside the Newton loop. Freezing the realization
    /// here gives the iteration a fixed target to converge onto, keeps the density from being
    /// modulated by the noise that it is generating, and pins the stochastic-calculus convention.
    /// </remarks>
    void Probe();

    /// <summary>
    /// Stamps the frozen realization into the right-hand side vector.
    /// </summary>
    /// <remarks>
    /// Called on every load of the circuit.
    /// </remarks>
    void Inject();
}
