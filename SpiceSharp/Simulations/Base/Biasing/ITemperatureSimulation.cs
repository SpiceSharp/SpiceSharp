using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can calculate temperature-dependent effects.
    /// </summary>
    /// <seealso cref="ISimulation"/>
    /// <seealso cref="IStateful{S}"/>
    /// <seealso cref="ITemperatureSimulationState"/>
    /// <seealso cref="IBehavioral{T}" />
    /// <seealso cref="ITemperatureBehavior"/>
    public interface ITemperatureSimulation : ISimulation,
        IStateful<ITemperatureSimulationState>,
        IBehavioral<ITemperatureBehavior>
    {
    }
}
