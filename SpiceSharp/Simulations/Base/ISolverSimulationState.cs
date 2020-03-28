using SpiceSharp.Algebra;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="ISimulationState"/> that describes a solver.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ISolverSimulationState<T> : ISimulationState, IVariableFactory<IVariable<T>> where T : IFormattable
    {
        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        ISparseSolver<T> Solver { get; }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        IVector<T> Solution { get; }

        /// <summary>
        /// Gets the map that maps variables to indices for the solver.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        IVariableMap Map { get; }
    }
}
