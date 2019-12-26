using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface that describes behaviors for biasing in a <see cref="IBiasingSimulation" />.
    /// This behavior is responsible for calculating the DC equivalent behavior of an entity, ie. for biasing the circuit.
    /// </summary>
    /// <seealso cref="IBehavior" />
    public interface IBiasingBehavior : IBehavior
    {
        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void Load();
    }
}
