using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    public abstract partial class FrequencySimulation
    {
        /// <summary>
        /// A simulation state using complex numbers.
        /// </summary>
        /// <seealso cref="IComplexSimulationState" />
        protected class ComplexSimulationState : VariableDictionary<IVariable<Complex>>, IComplexSimulationState
        {
            private readonly VariableMap _map;

            /// <summary>
            /// Gets the solution.
            /// </summary>
            public IVector<Complex> Solution { get; private set; }

            /// <summary>
            /// Gets or sets the current laplace variable.
            /// </summary>
            public Complex Laplace { get; set; } = new Complex();

            /// <summary>
            /// Gets the map that maps variables to indices for the solver.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map => _map;

            /// <summary>
            /// Gets the solver.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparseSolver<Complex> Solver { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="comparer">The comparer.</param>
            public ComplexSimulationState(ISparseSolver<Complex> solver, IEqualityComparer<string> comparer)
                : base(comparer)
            {
                Solver = solver.ThrowIfNull(nameof(solver));

                var gnd = new SolverVariable<Complex>(this, Constants.Ground, 0, Units.Volt);
                _map = new VariableMap(gnd);
                Add(Constants.Ground, gnd);
            }

            /// <summary>
            /// Maps a shared node in the simulation.
            /// </summary>
            /// <param name="name">The name of the shared node.</param>
            /// <returns>
            /// The shared node variable.
            /// </returns>
            public IVariable<Complex> GetSharedVariable(string name)
            {
                if (TryGetValue(name, out var result))
                    return result;

                // We create a private variable and then make it shared
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
            public IVariable<Complex> CreatePrivateVariable(string name, IUnit unit)
            {
                var index = _map.Count;
                var result = new SolverVariable<Complex>(this, name, index, unit);
                _map.Add(result, index);
                return result;
            }

            /// <summary>
            /// Set up the simulation state for the simulation.
            /// </summary>
            public void Setup()
            {
                Solution = new DenseVector<Complex>(Solver.Size);
            }
        }
    }
}
