using System;

namespace SpiceSharp.Simulations;

/// <summary>
/// A basic implementation of a <see cref="ITimeNoiseSource"/>. It owns the shaping state and the
/// random stream of one noise source, and leaves it to the deriving class to decide how the
/// realization reaches the circuit.
/// </summary>
/// <remarks>
/// <para>
/// The shaping state is kept at unit variance, and the stationary standard deviation of the source
/// is only applied when stamping. A moving operating point then does not look like a settling
/// artifact, and - more importantly - the coefficients of the state update contain nothing that is
/// specific to the source, which is what allows them to be shared through
/// <see cref="ITimeNoiseSimulationState.Point"/>.
/// </para>
/// <para>
/// The source registers itself as an <see cref="IIntegrationState"/>, so a rejected timepoint rolls
/// back for free: a probed timepoint is always propagated from the last accepted state, and only an
/// accepted timepoint promotes it.
/// </para>
/// </remarks>
/// <seealso cref="ITimeNoiseSource" />
/// <seealso cref="IIntegrationState" />
public abstract class TimeNoiseSource : ITimeNoiseSource, IIntegrationState
{
    private readonly ITimeNoiseSimulationState _state;
    private double[] _accepted, _probed;
    private NoiseRandomStream _stream;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public double NoiseDensity { get; protected set; }

    /// <inheritdoc/>
    public double Current { get; private set; }

    /// <summary>
    /// Gets or sets the stationary standard deviation of the noise current, in A.
    /// </summary>
    /// <value>
    /// The stationary standard deviation.
    /// </value>
    /// <remarks>
    /// Computed by the deriving class from the operating point of the device. It does not contain
    /// the timestep: the injected power is a property of the source and of the band limit alone.
    /// </remarks>
    protected double Amplitude { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoiseSource"/> class.
    /// </summary>
    /// <param name="name">The name of the noise source. It has to be unique within the circuit.</param>
    /// <param name="noise">The transient noise simulation state.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or
    /// <paramref name="noise"/> is <c>null</c>.</exception>
    protected TimeNoiseSource(string name, ITimeNoiseSimulationState noise)
    {
        Name = name.ThrowIfNull(nameof(name));
        _state = noise.ThrowIfNull(nameof(noise));

        _accepted = new double[TimeNoisePoint.MaximumOrder];
        _probed = new double[TimeNoisePoint.MaximumOrder];
        _stream = new NoiseRandomStream(0ul);
        _state.Register(this);
    }

    /// <summary>
    /// Restarts the noise source: reseeds its random stream, and draws a fresh shaping state from
    /// the stationary distribution of the shaping filter.
    /// </summary>
    /// <param name="seed">The seed of the random stream of this noise source.</param>
    /// <remarks>
    /// Starting from the stationary distribution rather than from zero avoids a startup transient
    /// that would look like a settling artifact over the first time constants of the shaping filter.
    /// </remarks>
    public virtual void Initialize(ulong seed)
    {
        _stream = new NoiseRandomStream(seed);
        Array.Clear(_accepted, 0, _accepted.Length);
        _stream.NextNormals(out double normal1, out double normal2);
        double value = TimeNoisePoint.Stationary(_state.BandLimitOrder)
            .Propagate(ref _accepted[0], ref _accepted[1], normal1, normal2);
        _accepted.CopyTo(_probed, 0);
        Current = Amplitude * value;
    }

    /// <summary>
    /// Advances the shaping state over the probed timestep, and freezes the noise current that is
    /// stamped until the next timepoint is probed.
    /// </summary>
    public virtual void Probe()
    {
        // Always propagate the last accepted state: the draws of a rejected timepoint are simply
        // discarded, and the retry starts over from the same state.
        _stream.NextNormals(out double normal1, out double normal2);
        _probed[0] = _accepted[0];
        _probed[1] = _accepted[1];
        Current = Amplitude * _state.Point.Propagate(ref _probed[0], ref _probed[1], normal1, normal2);
    }

    /// <summary>
    /// Stamps the frozen noise realization into the circuit.
    /// </summary>
    /// <remarks>
    /// How the source is wired into the circuit is left to the deriving class, so that the shaping
    /// machinery here does not have to assume that every noise source is a current between two
    /// nodes.
    /// </remarks>
    public abstract void Inject();

    /// <inheritdoc/>
    void IIntegrationState.Accept()
    {
        // Swap rather than copy. The arrays are never handed out, so nothing can alias the last
        // accepted state and destroy the rollback.
        (_accepted, _probed) = (_probed, _accepted);
    }
}
