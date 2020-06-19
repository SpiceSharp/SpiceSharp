using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface describing a class that can do noise analysis.
    /// </summary>
    /// <seealso cref="IFrequencySimulation" />
    /// <seealso cref="IBehavioral{T}" />
    /// <seealso cref="IStateful{T}" />
    public interface INoiseSimulation : IFrequencySimulation,
        IBehavioral<INoiseBehavior>,
        IStateful<INoiseSimulationState>
    {
    }
}
