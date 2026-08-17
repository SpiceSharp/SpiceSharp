using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Linq;

namespace SpiceSharp.Components.Subcircuits;

/// <summary>
/// An <see cref="ITimeNoiseBehavior"/> for a <see cref="SubcircuitDefinition"/>.
/// </summary>
/// <seealso cref="Time" />
/// <seealso cref="ITimeNoiseBehavior" />
[BehaviorFor(typeof(Subcircuit))]
public partial class TimeNoise : Time,
    ITimeNoiseBehavior
{
    private BehaviorList<ITimeNoiseBehavior> _noiseBehaviors;

    /// <inheritdoc/>
    public double NoiseDensity => _noiseBehaviors.Sum(nb => nb.NoiseDensity);

    /// <inheritdoc/>
    public double Current => _noiseBehaviors.Sum(nb => nb.Current);

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoise"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
    public TimeNoise(SubcircuitBindingContext context)
        : base(context)
    {
        context.AddLocalState<ITimeNoiseSimulationState>(
            new TimeNoiseSimulationState(Name, context.GetState<ITimeNoiseSimulationState>()));
    }

    /// <inheritdoc/>
    public override void FetchBehaviors(SubcircuitBindingContext context)
    {
        base.FetchBehaviors(context);
        _noiseBehaviors = context.GetBehaviors<ITimeNoiseBehavior>();
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.ProbeNoise()
    {
        foreach (var behavior in _noiseBehaviors)
            behavior.ProbeNoise();
    }
}
