using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can calculate temperature-dependent effects.
    /// </summary>
    /// <seealso cref="ISimulation" />
    /// <seealso cref="IBehavioral{T}" />
    public interface ITemperatureSimulation : IEventfulSimulation,
        IStateful<ITemperatureSimulationState>,
        IBehavioral<ITemperatureBehavior>
    {
    }
}
