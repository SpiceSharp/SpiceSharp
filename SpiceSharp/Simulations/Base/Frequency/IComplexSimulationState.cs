using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state using complex numbers.
    /// </summary>
    /// <seealso cref="ISimulationState"/>
    public interface IComplexSimulationState : ISolverSimulationState<Complex>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the solution converges.
        /// </summary>
        bool IsConvergent { get; set; }

        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        Complex Laplace { get; }
    }
}
