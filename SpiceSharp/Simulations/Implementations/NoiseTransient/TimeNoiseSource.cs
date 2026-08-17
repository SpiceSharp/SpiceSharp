using System;

namespace SpiceSharp.Simulations;

/// <summary>
/// A basic implementation of a <see cref="ITimeNoiseSource"/>. It owns the shaping state and the
/// random stream of one noise source, and leaves it to the deriving class to decide how the
/// realization reaches the circuit. The state is kept at unit variance.
/// </summary>
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
    protected double Amplitude { get; set; }

    /// <summary>
    /// Sets the one-sided power spectral density of the source, and the stationary standard
    /// deviation that follows from it.
    /// </summary>
    /// <param name="density">The one-sided power spectral density, in A^2/Hz.</param>
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
        State.Register(this, Name);
    }

    /// <summary>
    /// Restarts the noise source: reseeds its random stream, and draws a fresh shaping state from
    /// the stationary distribution of the shaping filter.
    /// </summary>
    /// <param name="seed">The seed of the random stream of this noise source.</param>
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
    public virtual void ProbeNoise()
    {
        Current = Amplitude * Shape(false);
    }

    /// <summary>
    /// Draws the next unit-variance output of the shaping filter of this source.
    /// </summary>
    /// <param name="initialize">If <c>true</c>, every section is drawn from its stationary
    /// distribution instead of being propagated over the probed timestep.</param>
    /// <returns>The unit-variance output of the shaping filter.</returns>
    protected virtual double Shape(bool initialize)
        => Propagate(0, initialize ? TimeNoisePoint.Stationary(State.BandLimitOrder) : State.Point);

    /// <summary>
    /// Advances one shaping section of this source over a point, and returns its unit-variance
    /// output.
    /// </summary>
    /// <param name="section">The index of the section.</param>
    /// <param name="point">The shaping coefficients of the section for the probed timestep.</param>
    /// <returns>The unit-variance output of the section.</returns>
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
    public abstract void InjectNoise();

    /// <inheritdoc/>
    void IIntegrationState.Accept()
    {
        // Swap rather than copy. The arrays are never handed out, so nothing can alias the last
        // accepted state and destroy the rollback.
        (_accepted, _probed) = (_probed, _accepted);
    }
}
