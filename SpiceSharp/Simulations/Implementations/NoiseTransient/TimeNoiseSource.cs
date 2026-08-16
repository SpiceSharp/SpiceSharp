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
    private double[] _accepted, _probed;
    private NoiseRandomStream _stream;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public double NoiseDensity { get; protected set; }

    /// <inheritdoc/>
    public double Current { get; protected set; }

    /// <summary>
    /// Gets the transient noise simulation state that this source draws its shaping coefficients
    /// from.
    /// </summary>
    /// <value>
    /// The transient noise simulation state.
    /// </value>
    protected ITimeNoiseSimulationState State { get; }

    /// <summary>
    /// Gets the number of shaping sections that this source holds.
    /// </summary>
    /// <value>
    /// The number of shaping sections.
    /// </value>
    protected int Sections { get; }

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
    /// Sets the one-sided power spectral density of the source, and the stationary standard
    /// deviation that follows from it.
    /// </summary>
    /// <param name="density">The one-sided power spectral density, in A^2/Hz.</param>
    /// <remarks>
    /// A band-limited source of density <paramref name="density"/> injects a total power of
    /// <c>k_n * density * fmax</c>, and <see cref="ITimeNoiseSimulationState.AmplitudeScale"/> is the
    /// square root of the part of that which does not depend on the source. It is therefore only
    /// meaningful for a source whose density is flat before the band limit shapes it, and a source
    /// with a density of its own sets <see cref="Amplitude"/> directly instead.
    /// </remarks>
    protected void SetNoiseDensity(double density)
    {
        NoiseDensity = density;
        Amplitude = State.AmplitudeScale * Math.Sqrt(density);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoiseSource"/> class with a single shaping
    /// section.
    /// </summary>
    /// <param name="name">The name of the noise source. It has to be unique within the circuit.</param>
    /// <param name="noise">The transient noise simulation state.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or
    /// <paramref name="noise"/> is <c>null</c>.</exception>
    protected TimeNoiseSource(string name, ITimeNoiseSimulationState noise)
        : this(name, noise, 1)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoiseSource"/> class.
    /// </summary>
    /// <param name="name">The name of the noise source. It has to be unique within the circuit.</param>
    /// <param name="noise">The transient noise simulation state.</param>
    /// <param name="sections">The number of shaping sections of the source.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="noise"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sections"/> is not positive.</exception>
    protected TimeNoiseSource(string name, ITimeNoiseSimulationState noise, int sections)
    {
        Name = name.ThrowIfNull(nameof(name));
        State = noise.ThrowIfNull(nameof(noise));
        if (sections < 1)
            throw new ArgumentOutOfRangeException(nameof(sections));
        Sections = sections;

        _accepted = new double[sections * TimeNoisePoint.MaximumOrder];
        _probed = new double[_accepted.Length];
        _stream = new NoiseRandomStream(0ul);
        State.Register(this);
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
        Current = Amplitude * Shape(true);

        // The drawn state is the one that the first probed timepoint propagates from.
        Array.Copy(_probed, _accepted, _probed.Length);
    }

    /// <summary>
    /// Advances the shaping state over the probed timestep, and freezes the noise current that is
    /// stamped until the next timepoint is probed.
    /// </summary>
    public virtual void Probe()
    {
        Current = Amplitude * Shape(false);
    }

    /// <summary>
    /// Draws the next unit-variance output of the shaping filter of this source.
    /// </summary>
    /// <param name="initialize">If <c>true</c>, every section is drawn from its stationary
    /// distribution instead of being propagated over the probed timestep.</param>
    /// <returns>
    /// The unit-variance output of the shaping filter.
    /// </returns>
    protected virtual double Shape(bool initialize)
        => Propagate(0, initialize ? TimeNoisePoint.Stationary(State.BandLimitOrder) : State.Point);

    /// <summary>
    /// Advances one shaping section of this source over a point, and returns its unit-variance
    /// output.
    /// </summary>
    /// <param name="section">The index of the section.</param>
    /// <param name="point">The shaping coefficients of the section for the probed timestep.</param>
    /// <returns>
    /// The unit-variance output of the section.
    /// </returns>

    protected double Propagate(int section, in TimeNoisePoint point)
    {
        int index = section * TimeNoisePoint.MaximumOrder;
        _probed[index] = _accepted[index];
        _probed[index + 1] = _accepted[index + 1];
        _stream.NextNormals(out double normal1, out double normal2);
        return point.Propagate(ref _probed[index], ref _probed[index + 1], normal1, normal2);
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
