using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.NoiseSources;

/// <summary>
/// A time-domain noise source that can be described by Johnson noise (thermal noise) models. It is a
/// noise current that is injected between two nodes.
/// </summary>
/// <remarks>
/// The time-domain counterpart of <see cref="NoiseThermal"/>. It produces the raw power spectral
/// density instead of one that has already been referred to the output of the circuit, because a
/// transient noise analysis stamps a current rather than solving an adjoint system.
/// </remarks>
/// <seealso cref="TimeNoiseSource"/>
public class TimeNoiseThermal : TimeNoiseSource
{
    private readonly double _scale;
    private readonly ElementSet<double> _elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoiseThermal" /> class.
    /// </summary>
    /// <param name="name">The name of the noise source. It has to be unique within the circuit.</param>
    /// <param name="noise">The transient noise simulation state.</param>
    /// <param name="biasing">The biasing simulation state.</param>
    /// <param name="pos">The positive node.</param>
    /// <param name="neg">The negative node.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments is <c>null</c>.</exception>
    public TimeNoiseThermal(string name, ITimeNoiseSimulationState noise, IBiasingSimulationState biasing,
        IVariable<double> pos, IVariable<double> neg)
        : base(name, noise)
    {
        biasing.ThrowIfNull(nameof(biasing));
        var variables = new OnePort<double>(pos, neg);
        _elements = new ElementSet<double>(biasing.Solver, null, variables.GetRhsIndices(biasing.Map));
        _scale = noise.AmplitudeScale;
    }

    /// <summary>
    /// Computes the Johnson or thermal noise density. This is 4 * k * T * G.
    /// </summary>
    /// <param name="conductance">The conductance.</param>
    /// <param name="temperature">The temperature.</param>
    public void Compute(double conductance, double temperature)
    {
        NoiseDensity = 4.0 * Constants.Boltzmann * temperature * conductance;
        Amplitude = _scale * Math.Sqrt(NoiseDensity);
    }

    /// <inheritdoc/>
    public override void Inject() => _elements.Add(-Current, Current);
}
