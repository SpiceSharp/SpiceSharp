using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state using complex numbers.
    /// </summary>
    /// <seealso cref="ISolverSimulationState{T}"/>
    public interface IComplexSimulationState : ISolverSimulationState<Complex>
    {
        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        /// <value>
        /// The laplace.
        /// </value>
        Complex Laplace { get; }
    }
}
