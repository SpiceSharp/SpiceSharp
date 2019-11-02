using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract class ParallelSolveSolverState<T> : ISolverSimulationState<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets the parent <see cref="IBiasingSimulationState"/>.
        /// </summary>
        /// <value>
        /// The parent state.
        /// </value>
        protected ISolverSimulationState<T> Parent { get; }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IVector<T> Solution { get; private set; }

        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISparseSolver<T> Solver { get; }

        /// <summary>
        /// Gets the map that maps <see cref="Variable" /> to indices for the solver.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map { get; }

        /// <summary>
        /// Private variables
        /// </summary>
        private List<ElementPair<T>> _syncPairs = new List<ElementPair<T>>();
        private Dictionary<int, int> _commonIndices = new Dictionary<int, int>();
        private bool _isPreordered = false;
        private bool _shouldReorder = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSolveSolverState{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="solver">The solver.</param>
        public ParallelSolveSolverState(ISolverSimulationState<T> parent, SparseLUSolver<SparseMatrix<T>, SparseVector<T>, T> solver)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            Solver = solver.ThrowIfNull(nameof(solver));
            Map = new VariableMap(parent.Map.Ground);
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Setup(ISimulation simulation)
        {
            Solution = new DenseVector<T>(Solver.Size);
            _isPreordered = false;
            _shouldReorder = true;
        }

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        public virtual void Unsetup()
        {
        }

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public virtual void Reset()
        {
            Solver.Reset();
            Solution[0] = default;
        }

        /// <summary>
        /// Notifies the state that these variables are shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public virtual void ShareVariables(HashSet<Variable> common)
        {
            int target = Solver.Size;
            _isPreordered = false;

            // We need to move any shared variable equations to the end
            foreach (var node in common)
            {
                if (node == Map.Ground)
                    continue;

                // Only apply for nodes that this state is using
                if (!Map.TryGetIndex(node, out var localIndex))
                    continue;
                if (!Parent.Map.TryGetIndex(node, out var globalIndex))
                    globalIndex = Parent.Map[node];
                _commonIndices.Add(localIndex, globalIndex);

                // Move the row/column to the end
                Solver.Precondition((matrix, vector) =>
                {
                    var loc = new MatrixLocation(localIndex, localIndex);
                    loc = Solver.ExternalToInternal(loc);
                    matrix.SwapRows(loc.Row, target);
                    matrix.SwapColumns(loc.Column, target);
                    target--;
                });
            }
            Solver.Order = target;
            ((SparseLUSolver<SparseMatrix<T>, SparseVector<T>, T>)Solver).Strategy.SearchLimit = target;

            // Map matrix elements
            foreach (var row in _commonIndices)
            {
                foreach (var column in _commonIndices)
                {
                    var localElt = Solver.GetElement(row.Key, column.Key);
                    var parentElt = Parent.Solver.GetElement(row.Value, column.Value);
                    _syncPairs.Add(new ElementPair<T>(localElt, parentElt));
                }
            }

            // Map vector elements
            bool createFillins = true;
            Solver.Precondition((m, v) =>
                createFillins = (((ISparseVector<T>)v).GetFirstInVector()?.Index ?? Solver.Size) < Solver.Order);
            foreach (var row in _commonIndices)
            {
                Element<T> localElt;
                if (createFillins)
                    localElt = Solver.GetElement(row.Key);
                else
                {
                    localElt = Solver.GetElement(row.Key);
                    if (localElt == null)
                        continue;
                }
                var parentElt = Parent.Solver.GetElement(row.Value);
                _syncPairs.Add(new ElementPair<T>(localElt, parentElt));
            }
        }

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public virtual void ApplySynchronously()
        {
            // Add the contributions to the parent solver
            foreach (var pairs in _syncPairs)
                pairs.Parent.Add(pairs.Local.Value);
        }

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        /// <returns>True if the application was succesful.</returns>
        public virtual bool ApplyAsynchronously()
        {
            try
            {
                if (!_isPreordered)
                {
                    Solver.Precondition((matrix, vector)
                        => ModifiedNodalAnalysisHelper<T>.PreorderModifiedNodalAnalysis(matrix, Solver.Order));
                    _isPreordered = true;
                }

                if (_shouldReorder)
                {
                    Solver.OrderAndFactor();
                    _shouldReorder = false;
                }
                else
                {
                    var success = Solver.Factor();
                    if (!success)
                    {
                        _shouldReorder = true;
                        return false;
                    }
                }
                return true;
            }
            catch (AlgebraException)
            {
                throw new CircuitException("Cannot parallelize subcircuit");
            }
        }

        /// <summary>
        /// Updates the state with the new solution.
        /// </summary>
        public virtual void Update()
        {
            // Copy the solution from the previous iteration
            foreach (var pair in _commonIndices)
                Solution[pair.Key] = Parent.Solution[pair.Value];
            Solver.Solve(Solution);
        }
    }
}
