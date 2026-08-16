using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.NoiseSources;

/// <summary>
/// A time-domain noise source that can be described using shot noise models. It is a noise current
/// that is injected between two nodes.
/// </summary>
/// <remarks>
/// The time-domain counterpart of <see cref="NoiseShot"/>. It produces the raw power spectral
/// density instead of one that has already been referred to the output of the circuit, because a
/// transient noise analysis stamps a current rather than solving an adjoint system.
/// </remarks>
/// <seealso cref="TimeNoiseCurrentSource"/>
public class TimeNoiseShot : TimeNoiseCurrentSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoiseShot" /> class.
    /// </summary>
    /// <param name="name">The name of the noise source. It has to be unique within the circuit.</param>
    /// <param name="noise">The transient noise simulation state.</param>
    /// <param name="biasing">The biasing simulation state.</param>
    /// <param name="pos">The positive node.</param>
    /// <param name="neg">The negative node.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments is <c>null</c>.</exception>
    public TimeNoiseShot(string name, ITimeNoiseSimulationState noise, IBiasingSimulationState biasing,
        IVariable<double> pos, IVariable<double> neg)
        : base(name, noise, biasing, pos, neg)
    {
    }

    /// <summary>
    /// Computes the shot noise density. This is 2 * q * |I|.
    /// </summary>
    /// <param name="current">The current.</param>
    public void Compute(double current)
        => SetNoiseDensity(2.0 * Constants.Charge * Math.Abs(current));
}
