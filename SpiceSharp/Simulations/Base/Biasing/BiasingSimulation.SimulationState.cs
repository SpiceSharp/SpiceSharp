using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Variables;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    public abstract partial class BiasingSimulation
    {
        /// <summary>
        /// A simulation state for simulations using real numbers.
        /// </summary>
        /// <seealso cref="IBiasingSimulationState" />
        private class SimulationState : VariableDictionary<IVariable<double>>, IBiasingSimulationState
        {
            private readonly VariableMap _map;

            /// <summary>
            /// Gets the solution vector.
            /// </summary>
            public IVector<double> Solution { get; private set; }

            /// <summary>
            /// Gets the previous solution vector.
            /// </summary>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution { get; private set; }

            /// <summary>
            /// Gets the map that maps variables to indices for the solver.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map => _map;

            /// <summary>
            /// Gets the sparse solver.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparsePivotingSolver<double> Solver { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="comparer">The comparer.</param>
            public SimulationState(ISparsePivotingSolver<double> solver, IEqualityComparer<string> comparer)
                : base(comparer)
            {
                Solver = solver.ThrowIfNull(nameof(solver));

                var gnd = new SolverVariable<double>(this, Constants.Ground, 0, Units.Volt);
                _map = new VariableMap(gnd);
                Add(Constants.Ground, gnd);
            }

            /// <summary>
            /// Gets a variable that can be shared with other behaviors by the factory. If another variable
            /// already exists with the same name, that is returned instead.
            /// </summary>
            /// <param name="name">The name of the shared variable.</param>
            /// <returns>
            /// The shared variable.
            /// </returns>
            public IVariable<double> GetSharedVariable(string name)
            {
                if (TryGetValue(name, out var result))
                    return result;

                // We create a private variable and then make it shared by adding it to the solved variable set
                result = CreatePrivateVariable(name, Units.Volt);
                Add(name, result);
                return result;
            }

            /// <summary>
            /// Creates a local variable that should not be shared by the state with anyone else.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="unit">The unit of the variable.</param>
            /// <returns>
            /// The local variable.
            /// </returns>
            public IVariable<double> CreatePrivateVariable(string name, IUnit unit)
            {
                var index = _map.Count;
                var result = new SolverVariable<double>(this, name, index, unit);
                _map.Add(result, index);
                return result;
            }

            /// <summary>
            /// Sets up the simulation state.
            /// </summary>
            public void Setup()
            {
                // Initialize all matrices
                Solution = new DenseVector<double>(Solver.Size);
                OldSolution = new DenseVector<double>(Solver.Size);
                Solver.Reset();
            }

            /// <summary>
            /// Stores the solution.
            /// </summary>
            public void StoreSolution()
            {
                var tmp = OldSolution;
                OldSolution = Solution;
                Solution = tmp;
            }
        }
    }
}
