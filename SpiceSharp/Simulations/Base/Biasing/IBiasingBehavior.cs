using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface that describes behaviors for biasing in a <see cref="IBiasingSimulation" />.
    /// This behavior is responsible for calculating the DC equivalent behavior of an entity, ie. for biasing the circuit.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface IBiasingBehavior : IBehavior
    {
        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown if the behavior can't load the matrix and/or right hand side vector.</exception>
        void Load();
    }
}
