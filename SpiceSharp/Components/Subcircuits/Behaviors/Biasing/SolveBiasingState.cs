using System;
using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Biasing state for <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <seealso cref="BiasingSimulationState" />
    public class SolveBiasingState : BiasingSimulationState
    {
        private List<ElementPair> _syncPairs = new List<ElementPair>();
        private Dictionary<int, int> _commonIndices = new Dictionary<int, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SolveBiasingState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public SolveBiasingState(IBiasingSimulationState parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Notifies the state that these variables can be shared with other states.
        /// If <paramref name="common"/> is null, all variables are considered to be shared.
        /// </summary>
        /// <param name="common">The common variables.</param>
        public override void ShareVariables(HashSet<Variable> common)
        {
            int target = Solver.Size;

            // We need to move any shared variables to the end of the solver
            foreach (var node in common)
            {
                if (node == Map.Ground)
                    continue;

                // Only apply for nodes that this state is using
                if (!Map.TryGetIndex(node, out var local_index))
                    continue;
                if (!Parent.Map.TryGetIndex(node, out var global_index))
                    global_index = Parent.Map[node];
                _commonIndices.Add(local_index, global_index);

                // Move the row and column to the last one
                Solver.Precondition((matrix, rhs) =>
                {
                    var loc = Solver.ExternalToInternal(new MatrixLocation(local_index, local_index));
                    matrix.SwapRows(loc.Row, target);
                    matrix.SwapColumns(loc.Column, target);
                    target--;
                });
            }
            LocalSolver.Order = target;
            LocalSolver.Strategy.SearchLimit = target;

            // Map matrix elements
            foreach (var row in _commonIndices)
            {
                foreach (var column in _commonIndices)
                {
                    var local_elt = LocalSolver.GetElement(row.Key, column.Key);
                    var global_elt = Parent.Solver.GetElement(row.Value, column.Value);
                    _syncPairs.Add(new ElementPair(local_elt, global_elt));
                }
            }

            // Map vector elements
            bool createFillins = true;
            LocalSolver.Precondition((m, v) =>
                createFillins = (((ISparseVector<double>)v).GetFirstInVector()?.Index ?? LocalSolver.Size) < LocalSolver.Order);
            foreach (var row in _commonIndices)
            {
                Element<double> local_elt;
                if (createFillins)
                    local_elt = LocalSolver.GetElement(row.Key);
                else
                {
                    local_elt = LocalSolver.FindElement(row.Key);
                    if (local_elt == null)
                        continue;
                }
                var global_elt = Parent.Solver.GetElement(row.Value);
                _syncPairs.Add(new ElementPair(local_elt, global_elt));
            }
        }

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        public override void ApplyAsynchroneously()
        {
            // Do a partial solve of the solver
            try
            {
                Solver.OrderAndFactor();
            }
            catch (AlgebraException)
            {
                throw new CircuitException("Cannot parallelize subcircuit");
            }
        }

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public override void ApplySynchroneously()
        {
            // Add the contributions to the parent solver
            foreach (var pairs in _syncPairs)
                pairs.Parent.Add(pairs.Local.Value);
        }

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        public override bool CheckConvergence()
        {
            // Copy the solution from the previous iteration to the local solution
            foreach (var pair in _commonIndices)
                Solution[pair.Key] = Parent.Solution[pair.Value];

            // Solve to our local solution for the other elements
            Solver.Solve(Solution);

            // Check for convergence on the variables, similar to BiasingSimulation.
            return true;
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Setup(ISimulation simulation)
        {
            base.Setup(simulation);
            Solution = new DenseVector<double>(Solver.Size);
            OldSolution = new DenseVector<double>(Solver.Size);
        }
    }
}
