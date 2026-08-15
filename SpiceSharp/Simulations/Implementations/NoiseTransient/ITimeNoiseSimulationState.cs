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
    /// with the integration method, and seeds its random stream from the master seed and the name
    /// of the source.
    /// </summary>
    /// <param name="source">The noise source.</param>
    void Register(TimeNoiseSource source);
}
