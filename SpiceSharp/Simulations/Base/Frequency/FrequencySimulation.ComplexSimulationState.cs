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
        protected class ComplexSimulationState : IComplexSimulationState
        {
            private readonly VariableMap _map;
            private readonly VariableSet<IVariable<Complex>> _solved;

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
            /// Initializes a new instance of the <see cref="ComplexSimulationState" /> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="solvedVariables">The set with solved variables.</param>
            public ComplexSimulationState(ISparseSolver<Complex> solver, VariableSet<IVariable<Complex>> solvedVariables)
            {
                Solver = solver.ThrowIfNull(nameof(solver));

                var gnd = new GroundVariable<Complex>();
                _map = new VariableMap(gnd);
                _solved = solvedVariables;
                _solved.Add(gnd);
            }

            /// <summary>
            /// Maps a shared node in the simulation.
            /// </summary>
            /// <param name="name">The name of the shared node.</param>
            /// <returns>
            /// The shared node variable.
            /// </returns>
            public IVariable<Complex> MapNode(string name)
            {
                if (_solved.TryGetValue(name, out var result))
                    return result;
                result = Create(name, Units.Volt);
                _solved.Add(result);
                return result;
            }

            /// <summary>
            /// Maps a number of nodes.
            /// </summary>
            /// <param name="names">The nodes.</param>
            /// <returns>
            /// The shared node variables.
            /// </returns>
            public IEnumerable<IVariable<Complex>> MapNodes(IEnumerable<string> names)
            {
                foreach (var name in names)
                    yield return MapNode(name);
            }

            /// <summary>
            /// Creates a local variable that should not be shared by the state with anyone else.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="unit">The unit of the variable.</param>
            /// <returns>
            /// The local variable.
            /// </returns>
            public IVariable<Complex> Create(string name, IUnit unit)
            {
                var result = new SolverVariable<Complex>(this, name, _map.Count, unit);
                _map.Add(result, _map.Count);
                return result;
            }

            /// <summary>
            /// Set up the simulation state for the simulation.
            /// </summary>
            public void Setup()
            {
                Solution = new DenseVector<Complex>(Solver.Size);
            }

            /// <summary>
            /// Determines whether the specified variable is a node without mapping it.
            /// </summary>
            /// <param name="name">The name of the node.</param>
            /// <returns>
            /// <c>true</c> if the specified variable has node; otherwise, <c>false</c>.
            /// </returns>
            public bool HasNode(string name) => _solved.ContainsKey(name);
        }
    }
}
