using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class BiasingBehavior
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState"/> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="IBiasingSimulationState" />
        protected class SimulationState : IBiasingSimulationState
        {
            private IBiasingSimulationState _parent;
            private readonly List<Bridge> _bridges = new List<Bridge>();
            private readonly Dictionary<int, int> _variableMap = new Dictionary<int, int>();
            private bool _shouldReorder = true, _shouldPreorder = true, _isUpdated;
            private int _sharedCount = 0;

            /// <summary>
            /// Gets or sets the initialization flag.
            /// </summary>
            public InitializationModes Init => _parent.Init;

            /// <summary>
            /// Gets or sets the flag for ignoring time-related effects. If true, each device should assume the circuit is not moving in time.
            /// </summary>
            public bool UseDc => _parent.UseDc;

            /// <summary>
            /// Gets or sets the flag for using initial conditions. If true, the operating point will not be calculated, and initial conditions will be used instead.
            /// </summary>
            public bool UseIc => _parent.UseIc;

            /// <summary>
            /// The current source factor.
            /// This parameter is changed when doing source stepping for aiding convergence.
            /// </summary>
            public double SourceFactor => _parent.SourceFactor;

            /// <summary>
            /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
            /// </summary>
            public double Gmin => _parent.Gmin;

            /// <summary>
            /// Is the current iteration convergent?
            /// This parameter is used to communicate convergence.
            /// </summary>
            public bool IsConvergent { get; set; }

            /// <summary>
            /// The current temperature for this circuit in Kelvin.
            /// </summary>
            public double Temperature { get; set; }

            /// <summary>
            /// The nominal temperature for the circuit in Kelvin.
            /// Used by models as the default temperature where the parameters were measured.
            /// </summary>
            public double NominalTemperature => _parent.NominalTemperature;

            /// <summary>
            /// Gets the previous solution vector.
            /// </summary>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution { get; private set; }

            /// <summary>
            /// Gets the solver used to solve the system of equations.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparseSolver<double> Solver { get; }

            /// <summary>
            /// Gets the solution.
            /// </summary>
            /// <value>
            /// The solution.
            /// </value>
            public IVector<double> Solution { get; private set; }

            /// <summary>
            /// Gets the map that maps <see cref="Variable" /> to indices for the solver.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent simulation state.</param>
            /// <param name="solver">The solver.</param>
            public SimulationState(IBiasingSimulationState parent, ISparseSolver<double> solver)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                Solver = solver.ThrowIfNull(nameof(solver));
                Map = new VariableMap(parent.Map.Ground);
            }

            /// <summary>
            /// Initializes the simulation state.
            /// </summary>
            public void Initialize(IEnumerable<Variable> shared)
            {
                Solution = new DenseVector<double>(Solver.Size);
                _shouldPreorder = true;
                _shouldReorder = true;
                _isUpdated = true;
                ReorderLocalSolver(shared);
            }

            /// <summary>
            /// Reorders the local solver.
            /// </summary>
            private void ReorderLocalSolver(IEnumerable<Variable> shared)
            {
                var solver = (SparseRealSolver<SparseMatrix<double>, SparseVector<double>>)Solver;
                _sharedCount = 0;
                solver.Precondition((matrix, vector) =>
                {
                    foreach (var variable in shared)
                    {
                        // Let's move this variable to the back
                        int index = Map[variable];
                        var location = solver.ExternalToInternal(new MatrixLocation(index, index));
                        int target = matrix.Size - _sharedCount;
                        matrix.SwapColumns(location.Column, target);
                        matrix.SwapRows(location.Row, target);
                        _sharedCount++;
                    }
                });
                solver.Degeneracy = _sharedCount;
                solver.Strategy.SearchReduction = _sharedCount;

                // Get the elements that need to be shared
                solver.Precondition((m, v) =>
                {
                    var matrix = (ISparseMatrix<double>)m;
                    var vector = (ISparseVector<double>)v;
                    foreach (var row in shared)
                    {
                        LinkElement(vector, row);
                        foreach (var col in shared)
                            LinkElement(matrix, row, col);
                    }
                });
            }

            /// <summary>
            /// Links vector elements.
            /// </summary>
            /// <param name="vector">The vector.</param>
            /// <param name="row">The row variable.</param>
            private void LinkElement(ISparseVector<double> vector, Variable row)
            {
                var local_index = Map[row];
                var parent_index = _parent.Map[row];
                _variableMap.Add(parent_index, local_index);

                // Do we need to create an element?
                var local_elt = Solver.FindElement(local_index);
                if (local_elt == null)
                {
                    // Check if solving will result in an element
                    var first = vector.GetFirstInVector();
                    if (first == null || first.Index > Solver.Size - _sharedCount)
                        return;
                    local_elt = vector.GetElement(local_index);
                }
                if (local_elt == null)
                    return;
                var parent_elt = _parent.Solver.GetElement(parent_index);
                _bridges.Add(new Bridge(local_elt, parent_elt));
            }

            /// <summary>
            /// Links matrix elements.
            /// </summary>
            /// <param name="matrix">The matrix.</param>
            /// <param name="row">The row variable.</param>
            /// <param name="column">The column variable.</param>
            private void LinkElement(ISparseMatrix<double> matrix, Variable row, Variable column)
            {
                var loc = Solver.ExternalToInternal(new MatrixLocation(Map[row], Map[column]));

                // Do we need to create an element?
                var local_elt = matrix.FindElement(loc.Row, loc.Column);
                if (local_elt == null)
                {
                    // Check if solving will result in an element
                    var left = matrix.GetFirstInRow(loc.Row);
                    if (left == null || left.Column > Solver.Size - _sharedCount)
                        return;

                    var top = matrix.GetFirstInColumn(loc.Column);
                    if (top == null || top.Row > Solver.Size - _sharedCount)
                        return;

                    // Create the element because decomposition will cause these elements to be created
                    local_elt = matrix.GetElement(loc.Row, loc.Column);
                }
                if (local_elt == null)
                    return;
                var parent_elt = _parent.Solver.GetElement(_parent.Map[row], _parent.Map[column]);
                _bridges.Add(new Bridge(local_elt, parent_elt));
            }

            /// <summary>
            /// Applies the local solver to the parent solver. If the application
            /// was unsuccessful, the solver should be loaded again.
            /// </summary>
            /// <returns>
            /// <c>true</c> if applied successfully; otherwise <c>false</c>.
            /// </returns>
            public bool Apply()
            {
                if (_shouldPreorder)
                {
                    Solver.Precondition((matrix, vector) =>
                        ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix, Solver.Size - Solver.Degeneracy));
                    _shouldPreorder = false;
                }
                if (_shouldReorder)
                {
                    // If the solver could not solve the whole matrix, throw an exception
                    if (Solver.OrderAndFactor() < Solver.Size - Solver.Degeneracy)
                        throw new NoEquivalentSubcircuitException();
                    _shouldReorder = false;
                }
                else
                {
                    if (!Solver.Factor())
                    {
                        _shouldReorder = true;
                        return false;
                    }
                }

                // Copy the necessary elements
                foreach (var bridge in _bridges)
                    bridge.Apply();
                _isUpdated = false;
                return true;
            }

            /// <summary>
            /// Updates this instance.
            /// </summary>
            public void Update()
            {
                // No need to update again
                if (_isUpdated)
                    return;

                // Fill in the shared variables from the parent and solve
                foreach (var pair in _variableMap)
                    Solution[pair.Value] = _parent.Solution[pair.Key];
                Solver.Solve(Solution);
                Solution[0] = 0.0;
                _isUpdated = true;
            }
        }
    }
}
