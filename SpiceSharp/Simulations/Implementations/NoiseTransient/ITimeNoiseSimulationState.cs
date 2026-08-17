using System.Collections.Generic;

namespace SpiceSharp.Simulations;

/// <summary>
/// A simulation state for a <see cref="NoiseTransient"/> analysis.
/// </summary>
/// <seealso cref="ISimulationState" />
public interface ITimeNoiseSimulationState : ISimulationState
{
    /// <summary>
    /// Gets the noise bandwidth limit, in Hz.
    /// </summary>
    /// <value>
    /// The maximum noise frequency.
    /// </value>
    double MaximumNoiseFrequency { get; }

    /// <summary>
    /// Gets the number of shaping poles that band-limit every noise source.
    /// </summary>
    /// <value>
    /// The number of shaping poles.
    /// </value>
    int BandLimitOrder { get; }

    /// <summary>
    /// Gets the band-limit shaping coefficients for the currently probed timestep.
    /// </summary>
    /// <value>
    /// The shaping coefficients.
    /// </value>
    /// <remarks>
    /// Shared by every noise source in the circuit.
    /// </remarks>
    TimeNoisePoint Point { get; }

    /// <summary>
    /// Gets the rates of the sections of the flicker noise ladder, in rad/s.
    /// </summary>
    /// <value>
    /// The rates of the ladder sections.
    /// </value>
    IReadOnlyList<double> FlickerRates { get; }

    /// <summary>
    /// Gets the shaping coefficients of every section of the flicker noise ladder for the currently
    /// probed timestep, indexed in lockstep with <see cref="FlickerRates"/>.
    /// </summary>
    /// <value>
    /// The shaping coefficients of the ladder sections.
    /// </value>
    IReadOnlyList<TimeNoisePoint> FlickerLadder { get; }

    /// <summary>
    /// Gets the amplitude weights that realize a <c>1/f^beta</c> roll-off on
    /// <see cref="FlickerLadder"/>, and activates the ladder.
    /// </summary>
    /// <param name="exponent">The roll-off exponent <c>beta</c>. It has to lie strictly between <see cref="FlickerWeights.MinimumExponent"/> and <see cref="FlickerWeights.MaximumExponent"/>.</param>
    /// <returns>The weights.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="exponent"/> is out of range.</exception>
    FlickerWeights GetFlickerWeights(double exponent);

    /// <summary>
    /// Gets the factor that converts the square root of a power spectral density into a stationary
    /// standard deviation, <c>sqrt(k_n * fmax)</c>.
    /// </summary>
    /// <value>
    /// The amplitude scale.
    /// </value>
    /// <remarks>
    /// <c>k_n</c> is the equivalent-noise-bandwidth factor of an n-pole shape, so that a source of
    /// one-sided density <c>S</c> injects a total power of <c>k_n * S * fmax</c>.
    /// </remarks>
    double AmplitudeScale { get; }

    /// <summary>
    /// Registers a noise source with the simulation. This allocates the shaping state of the source
    /// with the integration method, and seeds its random stream from the master seed and
    /// <paramref name="name"/>.
    /// </summary>
    /// <param name="source">The noise source.</param>
    /// <param name="name">The name that the random stream is seeded from. Must be unique.</param>
    void Register(TimeNoiseSource source, string name);
}
