using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="ISimulationState"/> that describes a simulation state that uses a solver for modified nodal analysis.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISimulationState"/>
    /// <seealso cref="IVariableFactory{V}"/>
    /// <seealso cref="IVariableDictionary{V}"/>
    /// <seealso cref="IVariable{T}"/>
    public interface ISolverSimulationState<T> :
        ISimulationState,
        IVariableFactory<IVariable<T>>,
        IVariableDictionary<IVariable<T>>
    {
        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        ISparsePivotingSolver<T> Solver { get; }

        /// <summary>
        /// Gets the solution to the solved equations.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        IVector<T> Solution { get; }

        /// <summary>
        /// Gets the <see cref="IVariableMap"/> that maps variables to indices for the solver.
        /// </summary>
        /// <value>
        /// The variable map.
        /// </value>
        IVariableMap Map { get; }
    }
}
