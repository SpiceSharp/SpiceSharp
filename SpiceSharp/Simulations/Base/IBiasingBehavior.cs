namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface that describes behaviors for biasing in a <see cref="SpiceSharp.Simulations.BaseSimulation" />.
    /// This behavior is responsible for calculating the DC equivalent behavior of an entity, ie. for biasing the circuit.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.IBehavior" />
    public interface IBiasingBehavior : IBehavior
    {
        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void Load();

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IsConvergent();
    }
}
