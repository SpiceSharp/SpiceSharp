using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Simulation state for a <see cref="IBiasingSimulation" />.
    /// </summary>
    /// <seealso cref="ISolverSimulationState{T}" />
    public interface IBiasingSimulationState : ISolverSimulationState<double>
    {
        /// <summary>
        /// Gets the solution vector of the last computed iteration.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        /// <value>
        /// The solution to the last iteration.
        /// </value>
        IVector<double> OldSolution { get; }
    }
}
