using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="ISolverSimulationState{T}"/> that is capable of loading in parallel.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract partial class ParallelLoadSolverState<T> : ISolverSimulationState<T> where T : IFormattable
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
        public IVector<T> Solution => Parent.Solution;

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
        private List<ElementPair<T>> _asyncPairs = new List<ElementPair<T>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelLoadSolverState{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent state.</param>
        /// <param name="solver">The local solver.</param>
        protected ParallelLoadSolverState(ISolverSimulationState<T> parent, ISparseSolver<T> solver)
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
        }

        /// <summary>
        /// Notifies the state that these variables are shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public virtual void ShareVariables(HashSet<Variable> common)
        {
            Variable[] elts = new Variable[Map.Count];
            foreach (var elt in Map)
                elts[elt.Value] = elt.Key;
            _syncPairs.Clear();
            _asyncPairs.Clear();

            Solver.Precondition((m, v) =>
            {
                var matrix = (ISparseMatrix<T>)m;
                var vector = (ISparseVector<T>)v;

                // Make pairs for all elements
                for (var r = 1; r <= Solver.Size; r++)
                {
                    var mElt = matrix.GetFirstInRow(r);
                    while (mElt != null)
                    {
                        // Get the row and column variables
                        var loc = new MatrixLocation(mElt.Row, mElt.Column);
                        loc = Solver.InternalToExternal(loc);
                        var rowVariable = elts[loc.Row];
                        var colVariable = elts[loc.Column];

                        // Get the local element and the parent element
                        var localElt = (Element<T>)mElt;
                        var parentElt = Parent.Solver.GetElement(Parent.Map[rowVariable], Parent.Map[colVariable]);

                        if (common.Contains(rowVariable) || common.Contains(colVariable))
                            _syncPairs.Add(new ElementPair<T>(localElt, parentElt));
                        else
                            _asyncPairs.Add(new ElementPair<T>(localElt, parentElt));
                        mElt = mElt.Right;
                    }
                }

                var vElt = vector.GetFirstInVector();
                while (vElt != null)
                {
                    // Get the row variable
                    var loc = new MatrixLocation(vElt.Index, 1);
                    loc = Solver.InternalToExternal(loc);
                    var rowVariable = elts[loc.Row];

                    // Get the local element and the parent element
                    var localElt = (Element<T>)vElt;
                    var parentElt = Parent.Solver.GetElement(Parent.Map[rowVariable]);

                    if (common.Contains(rowVariable))
                        _syncPairs.Add(new ElementPair<T>(localElt, parentElt));
                    else
                        _asyncPairs.Add(new ElementPair<T>(localElt, parentElt));
                    vElt = vElt.Below;
                }
            });
        }

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public virtual void ApplySynchronously()
        {
            foreach (var elt in _syncPairs)
                elt.Parent.Add(elt.Local.Value);
        }

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        /// <returns>True if the application was succesful.</returns>
        public virtual bool ApplyAsynchronously()
        {
            foreach (var elt in _asyncPairs)
                elt.Parent.Add(elt.Local.Value);
            return true;
        }

        /// <summary>
        /// Updates the state with the new solution.
        /// </summary>
        public virtual void Update()
        {
        }
    }
}
