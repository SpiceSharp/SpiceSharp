using SpiceSharp.Behaviors;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes a class that can do small-signal analysis.
    /// </summary>
    /// <seealso cref="IBiasingSimulation"/>
    /// <seealso cref="ISimulation{V}"/>
    /// <seealso cref="IVariable{T}"/>
    /// <seealso cref="IBehavioral{B}"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IStateful{S}"/>
    /// <seealso cref="IComplexSimulationState"/>
    public interface IFrequencySimulation :
        IBiasingSimulation,
        ISimulation<IVariable<Complex>>,
        IBehavioral<IFrequencyBehavior>,
        IStateful<IComplexSimulationState>
    {
    }
}
