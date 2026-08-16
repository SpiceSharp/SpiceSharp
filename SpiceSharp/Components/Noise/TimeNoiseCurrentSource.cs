using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.NoiseSources;

/// <summary>
/// A time-domain noise source that reaches the circuit as a noise current between two nodes.
/// </summary>
/// <seealso cref="TimeNoiseSource"/>
public abstract class TimeNoiseCurrentSource : TimeNoiseSource
{
    private readonly ElementSet<double> _elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoiseCurrentSource" /> class.
    /// </summary>
    /// <param name="name">The name of the noise source. It has to be unique within the circuit.</param>
    /// <param name="noise">The transient noise simulation state.</param>
    /// <param name="biasing">The biasing simulation state.</param>
    /// <param name="pos">The positive node.</param>
    /// <param name="neg">The negative node.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments is <c>null</c>.</exception>
    protected TimeNoiseCurrentSource(string name, ITimeNoiseSimulationState noise, IBiasingSimulationState biasing,
        IVariable<double> pos, IVariable<double> neg)
        : base(name, noise)
    {
        biasing.ThrowIfNull(nameof(biasing));
        var variables = new OnePort<double>(pos, neg);
        _elements = new ElementSet<double>(biasing.Solver, null, variables.GetRhsIndices(biasing.Map));
    }

    /// <inheritdoc/>
    public override void Inject() => _elements.Add(-Current, Current);
}
