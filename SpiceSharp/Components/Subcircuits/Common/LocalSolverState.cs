using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A simulation state that has a local solver.
    /// </summary>
    /// <typeparam name="T">The solver value type.</typeparam>
    /// <typeparam name="S">The parent simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract partial class LocalSolverState<T, S> : SubcircuitSolverState<T, S>
        where S : ISolverSimulationState<T>
    {
        private bool _shouldPreorder, _shouldReorder;
        private readonly List<Bridge<Element<T>>> _matrixElements = new List<Bridge<Element<T>>>();
        private readonly Dictionary<int, Bridge<Element<T>>> _rhsElements = new Dictionary<int, Bridge<Element<T>>>();
        private readonly List<Bridge<int>> _nodeIndices = new List<Bridge<int>>();
        private readonly VariableMap _map;

        /// <summary>
        /// The local solution vector.
        /// </summary>
        protected IVector<T> LocalSolution;

        /// <inheritdoc/>
        public override ISparsePivotingSolver<T> Solver { get; }

        /// <inheritdoc/>
        public override IVector<T> Solution => LocalSolution;

        /// <inheritdoc/>
        public override IVariableMap Map => _map;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LocalSolverState{T,S}"/> has updated its solution.
        /// </summary>
        /// <value>
        ///   <c>true</c> if updated; otherwise, <c>false</c>.
        /// </value>
        protected bool Updated { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSolverState{T,S}"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit instance.</param>
        /// <param name="parent">The parent simulation state.</param>
        /// <param name="solver">The local solver.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/> or <paramref name="solver"/> is <c>null</c>.</exception>
        protected LocalSolverState(string name, S parent, ISparsePivotingSolver<T> solver)
            : base(name, parent)
        {
            Solver = solver.ThrowIfNull(nameof(solver));

            // We will keep our own map! We will use the parent's variable set though, to make sure our parent can find back
            // our solved variables.
            _map = new VariableMap(parent.Map[0]);
        }

        /// <summary>
        /// Initializes the specified shared.
        /// </summary>
        /// <param name="nodes">The node map.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is <c>null</c>.</exception>
        public virtual void Initialize(IReadOnlyList<Bridge<string>> nodes)
        {
            LocalSolution = new DenseVector<T>(Solver.Size);
            _shouldPreorder = true;
            _shouldReorder = true;
            Updated = true;
            ReorderLocalSolver(nodes);
        }

        /// <summary>
        /// Reorders the local solver.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> is <c>null</c>.</exception>
        private void ReorderLocalSolver(IReadOnlyList<Bridge<string>> nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));

            // Map each local node on the global node
            _nodeIndices.Clear();
            foreach (var bridge in nodes)
            {
                var loc = GetSharedVariable(bridge.Local);
                var glob = Parent.GetSharedVariable(bridge.Global);
                _nodeIndices.Add(new Bridge<int>(_map[loc], Parent.Map[glob]));
            }

            Solver.Precondition((matrix, vector) =>
            {
                // Create diagonal elements if the matrix is too small
                // This can happen if there are only contributions to the right-hand side.
                for (int i = matrix.Size + 1; i <= vector.Length; i++)
                    matrix.GetElement(new(i, i));

                for (int i = 0; i < _nodeIndices.Count; i++)
                {
                    // Let's move these variables to the back of the matrix
                    int index = _nodeIndices[i].Local;
                    var location = Solver.ExternalToInternal(new MatrixLocation(index, index));
                    int target = matrix.Size - i;
                    matrix.SwapColumns(location.Column, target);
                    matrix.SwapRows(location.Row, target);
                }
            });
            Solver.Degeneracy = _nodeIndices.Count;
            Solver.PivotSearchReduction = _nodeIndices.Count;

            // Get the elements that need to be shared
            Solver.Precondition((matrix, vector) =>
            {
                foreach (var row in _nodeIndices)
                {
                    LinkElement(vector, row);
                    foreach (var col in _nodeIndices)
                        LinkElement(matrix, row, col);
                }
            });
        }

        /// <summary>
        /// Links vector elements.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="row">The row variable.</param>
        private void LinkElement(ISparseVector<T> vector, Bridge<int> row)
        {
            var loc = Solver.ExternalToInternal(new MatrixLocation(row.Local, row.Local));

            // Do we need to create an element?
            var local_elt = vector.GetElement(loc.Row); // Always allocate element
            if (local_elt == null)
            {
                // Check if solving will result in an element
                var first = vector.GetFirstInVector();
                if (first == null || first.Index > Solver.Size - Solver.Degeneracy)
                    return;
                local_elt = vector.GetElement(loc.Row);
            }
            if (local_elt == null)
                return;
            var parent_elt = Parent.Solver.GetElement(row.Global);
            _rhsElements.Add(row.Local, new Bridge<Element<T>>(local_elt, parent_elt));
        }

        /// <summary>
        /// Links matrix elements.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="row">The row variable.</param>
        /// <param name="column">The column variable.</param>
        private void LinkElement(ISparseMatrix<T> matrix, Bridge<int> row, Bridge<int> column)
        {
            var loc = Solver.ExternalToInternal(new MatrixLocation(row.Local, column.Local));

            // Do we need to create an element?
            var local_elt = matrix.FindElement(loc);
            if (local_elt == null)
            {
                // Check if solving will result in an element
                var left = matrix.GetFirstInRow(loc.Row);
                if (left == null || left.Column > Solver.Size - Solver.Degeneracy)
                    return;

                var top = matrix.GetFirstInColumn(loc.Column);
                if (top == null || top.Row > Solver.Size - Solver.Degeneracy)
                    return;

                // Create the element because decomposition will cause these elements to be created
                local_elt = matrix.GetElement(loc);
            }
            if (local_elt == null)
                return;
            var parent_elt = Parent.Solver.GetElement(new MatrixLocation(row.Global, column.Global));
            _matrixElements.Add(new Bridge<Element<T>>(local_elt, parent_elt));
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

            // Copy the necessary elements from the matrix
            foreach (var bridge in _matrixElements)
                bridge.Global.Add(bridge.Local.Value);

            // Apply contributions
            Solver.ForwardSubstitute(Solution);
            foreach (var pair in _rhsElements)
            {
                // Add the RHS element directly
                pair.Value.Global.Add(pair.Value.Local.Value);
                pair.Value.Global.Subtract(Solver.ComputeDegenerateContribution(pair.Key));
            }
            Updated = false;
            return true;
        }

        /// <summary>
        /// Applies forward substitution for the transposed/adjoint matrix.
        /// </summary>
        public virtual void ApplyTransposed()
        {
            // Apply contributions
            Solver.ForwardSubstitute(Solution);
            foreach (var pair in _rhsElements)
            {
                // Add the RHS element directly
                pair.Value.Global.Add(pair.Value.Local.Value);
                pair.Value.Global.Subtract(Solver.ComputeDegenerateContributionTransposed(pair.Key));
            }
            Updated = false;
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
            foreach (var pair in _nodeIndices)
                Solution[pair.Local] = Parent.Solution[pair.Global];
            Solver.BackwardSubstitute(Solution);
            Solution[0] = default;
            Updated = true;
        }

        /// <summary>
        /// Updates the state with the new solution for the transposed version.
        /// </summary>
        public virtual void UpdateTransposed()
        {
            // No need to update again
            if (Updated)
                return;

            // Fill in the sahred variables
            foreach (var pair in _nodeIndices)
                Solution[pair.Local] = Parent.Solver[pair.Global];
            Solver.BackwardSubstituteTransposed(Solution);
            Solution[0] = default;
            Updated = true;
        }

        /// <inheritdoc/>
        public override IVariable<T> GetSharedVariable(string name)
        {
            // Get the local node!
            if (!TryGetValue(name, out var result))
            {
                int index = _map.Count;
                result = new SolverVariable<T>(this, name, index, Units.Volt);
                Add(name, result);
                _map.Add(result, index);
            }
            return result;
        }

        /// <inheritdoc/>
        public override IVariable<T> CreatePrivateVariable(string name, IUnit unit)
        {
            int index = _map.Count;
            var result = new SolverVariable<T>(this, name, index, unit);
            _map.Add(result, index);
            return result;
        }
    }
}
