using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A simulation state that has a local solver.
    /// </summary>
    /// <typeparam name="T">The solver value type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract partial class LocalSolverState<T> : ISolverSimulationState<T> where T : IFormattable, IEquatable<T>
    {
        private bool _shouldPreorder, _shouldReorder;
        private int _sharedCount;
        private readonly List<Bridge> _bridges = new List<Bridge>();
        private readonly Dictionary<int, int> _variableMap = new Dictionary<int, int>();
        private readonly ISolverSimulationState<T> _parent;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LocalSolverState{T}"/> has updated its solution.
        /// </summary>
        /// <value>
        ///   <c>true</c> if updated; otherwise, <c>false</c>.
        /// </value>
        protected bool Updated { get; private set; }

        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISparseSolver<T> Solver { get; }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IVector<T> Solution { get; protected set; }

        /// <summary>
        /// Gets the map that maps <see cref="Variable" /> to indices for the solver.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSolverState{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation state.</param>
        /// <param name="solver">The local solver.</param>
        protected LocalSolverState(ISolverSimulationState<T> parent, ISparseSolver<T> solver)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            Solver = solver.ThrowIfNull(nameof(solver));
            Map = new VariableMap(parent.MapNode(Constants.Ground));
        }

        /// <summary>
        /// Initializes the specified shared.
        /// </summary>
        /// <param name="shared">The shared variables.</param>
        public virtual void Initialize(IEnumerable<IVariable> shared)
        {
            Solution = new DenseVector<T>(Solver.Size);
            _shouldPreorder = true;
            _shouldReorder = true;
            Updated = true;
            ReorderLocalSolver(shared);
        }

        /// <summary>
        /// Reorders the local solver.
        /// </summary>
        /// <param name="shared">The shared variables.</param>
        private void ReorderLocalSolver(IEnumerable<IVariable> shared)
        {
            _sharedCount = 0;
            Solver.Precondition((matrix, vector) =>
            {
                foreach (var variable in shared)
                {
                    // Let's move these variables to the back of the matrix
                    int index = Map[variable];
                    var location = Solver.ExternalToInternal(new MatrixLocation(index, index));
                    int target = matrix.Size - _sharedCount;
                    matrix.SwapColumns(location.Column, target);
                    matrix.SwapRows(location.Row, target);
                    _sharedCount++;
                }
            });
            Solver.Degeneracy = _sharedCount;
            Solver.PivotSearchReduction = _sharedCount;

            // Get the elements that need to be shared
            Solver.Precondition((m, v) =>
            {
                var matrix = (ISparseMatrix<T>)m;
                var vector = (ISparseVector<T>)v;
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
        private void LinkElement(ISparseVector<T> vector, IVariable row)
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
        private void LinkElement(ISparseMatrix<T> matrix, IVariable row, IVariable column)
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
        /// Applies the local solver to the parent solver.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the application was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NoEquivalentSubcircuitException">Thrown if no equivalent contributions could be calculated.</exception>
        public virtual bool Apply()
        {
            if (_shouldPreorder)
            {
                Solver.Precondition((matrix, vector) =>
                    ModifiedNodalAnalysisHelper<T>.PreorderModifiedNodalAnalysis(matrix, Solver.Size - Solver.PivotSearchReduction));
                _shouldPreorder = false;
            }
            if (_shouldReorder)
            {
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
            Updated = false;
            return true;
        }

        /// <summary>
        /// Updates the state with the new solution.
        /// </summary>
        public virtual void Update()
        {
            // No need to update again
            if (Updated)
                return;

            // Fill in the shared variables
            foreach (var pair in _variableMap)
                Solution[pair.Value] = _parent.Solution[pair.Key];
            Solver.Solve(Solution);
            Solution[0] = default;
            Updated = true;
        }

        public IVariable<T> MapNode(string name)
        {
            throw new NotImplementedException();
        }

        public IVariable<T> Create(string name, IUnit unit)
        {
            throw new NotImplementedException();
        }

        public bool HasNode(string name)
        {
            return _parent.HasNode(name);
        }
    }
}
