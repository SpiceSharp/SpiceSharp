using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes a class that can do small-signal analysis.
    /// </summary>
    /// <seealso cref="IBiasingSimulation" />
    /// <seealso cref="IBehavioral{T}" />
    /// <seealso cref="IStateful{T}" />
    public interface IFrequencySimulation : IBiasingSimulation,
        IBehavioral<IFrequencyBehavior>,
        IStateful<IComplexSimulationState>
    {
    }
}
