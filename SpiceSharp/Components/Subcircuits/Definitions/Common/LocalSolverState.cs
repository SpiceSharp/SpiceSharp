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
    /// <typeparam name="S">The parent simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract partial class LocalSolverState<T, S> : ISolverSimulationState<T>
        where S : ISolverSimulationState<T>
        where T : IFormattable
    {
        private readonly string _name;
        private bool _shouldPreorder, _shouldReorder;
        private readonly List<Bridge<Element<T>>> _bridges = new List<Bridge<Element<T>>>();
        private readonly Dictionary<int, int> _variableMap = new Dictionary<int, int>();
        private readonly Dictionary<string, string> _nodeMap;
        private readonly VariableMap _map;

        /// <summary>
        /// The parent simulation state.
        /// </summary>
        protected readonly S Parent;

        /// <summary>
        /// Gets all shared variables.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        public IVariableSet<IVariable<T>> Variables => Parent.Variables;

        IVariableSet IVariableFactory.Variables => Parent.Variables;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LocalSolverState{T,S}"/> has updated its solution.
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
        public IVariableMap Map => _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSolverState{T,S}"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit instance.</param>
        /// <param name="nodes">The nodes.</param>
        /// <param name="parent">The parent simulation state.</param>
        /// <param name="solver">The local solver.</param>
        protected LocalSolverState(string name, IEnumerable<Bridge<string>> nodes, S parent, ISparseSolver<T> solver)
        {
            _name = name.ThrowIfNull(nameof(name));
            Parent = parent;
            Solver = solver.ThrowIfNull(nameof(solver));
            _map = new VariableMap(parent.GetSharedVariable(Constants.Ground));

            _nodeMap = new Dictionary<string, string>(parent.Variables.Comparer);
            _nodeMap.Add(Constants.Ground, Constants.Ground);
            foreach (var bridge in nodes)
                _nodeMap.Add(bridge.Local, bridge.Global);
        }

        /// <summary>
        /// Initializes the specified shared.
        /// </summary>
        /// <param name="nodes">The node map.</param>
        public virtual void Initialize(IReadOnlyList<Bridge<string>> nodes)
        {
            Solution = new DenseVector<T>(Solver.Size);
            _shouldPreorder = true;
            _shouldReorder = true;
            Updated = true;
            ReorderLocalSolver(nodes);
        }

        /// <summary>
        /// Reorders the local solver.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        private void ReorderLocalSolver(IReadOnlyList<Bridge<string>> nodes)
        {
            // Create a list of shared variables
            var shared = new IVariable<T>[nodes.Count];

            Solver.Precondition((matrix, vector) =>
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    shared[i] = GetSharedVariable(nodes[i].Local);

                    // Let's move these variables to the back of the matrix
                    int index = Map[shared[i]];
                    var location = Solver.ExternalToInternal(new MatrixLocation(index, index));
                    int target = matrix.Size - i;
                    matrix.SwapColumns(location.Column, target);
                    matrix.SwapRows(location.Row, target);
                }
            });
            Solver.Degeneracy = shared.Length;
            Solver.PivotSearchReduction = shared.Length;

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
            var parent_index = Parent.Map[row];
            _variableMap.Add(parent_index, local_index);

            // Do we need to create an element?
            var local_elt = Solver.FindElement(local_index);
            if (local_elt == null)
            {
                // Check if solving will result in an element
                var first = vector.GetFirstInVector();
                if (first == null || first.Index > Solver.Size - Solver.Degeneracy)
                    return;
                local_elt = vector.GetElement(local_index);
            }
            if (local_elt == null)
                return;
            var parent_elt = Parent.Solver.GetElement(parent_index);
            _bridges.Add(new Bridge<Element<T>>(local_elt, parent_elt));
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
                if (left == null || left.Column > Solver.Size - Solver.Degeneracy)
                    return;

                var top = matrix.GetFirstInColumn(loc.Column);
                if (top == null || top.Row > Solver.Size - Solver.Degeneracy)
                    return;

                // Create the element because decomposition will cause these elements to be created
                local_elt = matrix.GetElement(loc.Row, loc.Column);
            }
            if (local_elt == null)
                return;
            var parent_elt = Parent.Solver.GetElement(Parent.Map[row], Parent.Map[column]);
            _bridges.Add(new Bridge<Element<T>>(local_elt, parent_elt));
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
                bridge.Global.Add(bridge.Local.Value);
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
                Solution[pair.Value] = Parent.Solution[pair.Key];
            Solver.Solve(Solution);
            Solution[0] = default;
            Updated = true;
        }

        /// <summary>
        /// Maps a shared node in the simulation.
        /// </summary>
        /// <param name="name">The name of the shared node.</param>
        /// <returns>
        /// The shared node variable.
        /// </returns>
        public IVariable<T> GetSharedVariable(string name)
        {
            IVariable<T> result;
            if (_nodeMap.TryGetValue(name, out var mapped))
            {
                // The variable is a global pin node, so let's get it in from the parent state!
                result = Parent.GetSharedVariable(mapped);
                if (!_map.Contains(result))
                    _map.Add(result, _map.Count);
            }
            else
            {
                mapped = _name.Combine(name);
                if (!Variables.TryGetValue(mapped, out result))
                {
                    int index = _map.Count;
                    result = new SolverVariable<T>(this, mapped, index, Units.Volt);
                    _map.Add(result, index);
                    Variables.Add(result);
                }
            }
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
        public IVariable<T> CreatePrivateVariable(string name, IUnit unit)
        {
            var index = _map.Count;
            var result = new SolverVariable<T>(this, _name.Combine(name), index, unit);
            _map.Add(result, index);
            return result;
        }

        IVariable IVariableFactory.GetSharedVariable(string name) => GetSharedVariable(name);
        IVariable IVariableFactory.CreatePrivateVariable(string name, IUnit unit) => CreatePrivateVariable(name, unit);
    }
}
