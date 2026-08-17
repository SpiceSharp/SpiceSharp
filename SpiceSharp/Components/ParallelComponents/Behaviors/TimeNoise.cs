using SpiceSharp.Behaviors;
using System;
using System.Linq;

namespace SpiceSharp.Components.ParallelComponents;

/// <summary>
/// An <see cref="ITimeNoiseBehavior"/> for a <see cref="Parallel"/>.
/// </summary>
/// <seealso cref="Convergence" />
/// <seealso cref="ITimeNoiseBehavior" />
public class TimeNoise : Convergence,
    ITimeNoiseBehavior
{
    private readonly Workload _probeWorkload;
    private BehaviorList<ITimeNoiseBehavior> _noiseBehaviors;

    /// <inheritdoc/>
    public double NoiseDensity => _noiseBehaviors.Sum(nb => nb.NoiseDensity);

    /// <inheritdoc/>
    public double Current => _noiseBehaviors.Sum(nb => nb.Current);

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoise" /> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
    public TimeNoise(ParallelBindingContext context)
        : base(context)
    {
        var parameters = context.GetParameterSet<Parameters>();
        if (parameters.WorkDistributors.TryGetValue(typeof(ITimeNoiseBehavior), out var dist) && dist != null)
            _probeWorkload = new Workload(dist, parameters.Entities.Count);
    }

    /// <inheritdoc/>
    public override void FetchBehaviors(ParallelBindingContext context)
    {
        base.FetchBehaviors(context);
        _noiseBehaviors = context.GetBehaviors<ITimeNoiseBehavior>();
        if (_probeWorkload != null)
        {
            foreach (var behavior in _noiseBehaviors)
                _probeWorkload.Actions.Add(behavior.ProbeNoise);
        }
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.ProbeNoise()
    {
        if (_probeWorkload != null)
            _probeWorkload.Execute();
        else
        {
            foreach (var behavior in _noiseBehaviors)
                behavior.ProbeNoise();
        }
    }
}
