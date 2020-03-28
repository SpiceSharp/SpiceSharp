using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface describing a simulation that can calculate the biasing state of a circuit.
    /// </summary>
    /// <seealso cref="ITemperatureSimulation" />
    /// <seealso cref="IBehavioral{T}" />
    /// <seealso cref="IStateful{T}" />
    public interface IBiasingSimulation : ISimulation<IVariable<double>>,
        ITemperatureSimulation,
        IBehavioral<IBiasingBehavior>,
        IBehavioral<IConvergenceBehavior>,
        IStateful<IBiasingSimulationState>
    {
    }
}
