using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface describing a simulation that can calculate the biasing state of a circuit.
    /// </summary>
    /// <seealso cref="ISimulation{T}" />
    /// <seealso cref="ITemperatureSimulation" />
    /// <seealso cref="IBehavioral{B}" />
    /// <seealso cref="IStateful{S}" />
    /// <seealso cref="IBiasingSimulationState"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    public interface IBiasingSimulation : ISimulation<IVariable<double>>,
        ITemperatureSimulation,
        IBehavioral<IBiasingBehavior>,
        IBehavioral<IConvergenceBehavior>,
        IStateful<IBiasingSimulationState>
    {
    }
}
