using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes a class that can do transient analysis.
    /// </summary>
    /// <seealso cref="IBiasingSimulation" />
    /// <seealso cref="IBehavioral{T}" />
    /// <seealso cref="IStateful{T}" />
    public interface ITimeSimulation : IBiasingSimulation,
        IBehavioral<ITimeBehavior>,
        IStateful<IIntegrationMethod>,
        IStateful<ITimeSimulationState>
    {
    }
}
