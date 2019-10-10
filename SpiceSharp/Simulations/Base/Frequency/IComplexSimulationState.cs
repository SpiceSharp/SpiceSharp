using SpiceSharp.Algebra;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state using complex numbers.
    /// </summary>
    /// <seealso cref="ISimulationState"/>
    public interface IComplexSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the solution converges.
        /// </summary>
        bool IsConvergent { get; set; }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        IVector<Complex> Solution { get; }

        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        Complex Laplace { get; }

        /// <summary>
        /// Gets the solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        ISparseSolver<Complex> Solver { get; }
    }
}
