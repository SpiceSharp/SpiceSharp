using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Simulation state for a <see cref="IBiasingSimulation"/>.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface IBiasingSimulationState : ISolverSimulationState<double>
    {
        /// <summary>
        /// The current temperature for this circuit in Kelvin.
        /// </summary>
        double Temperature { get; set; }

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        double NominalTemperature { get; }

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        IVector<double> OldSolution { get; }
    }
}
