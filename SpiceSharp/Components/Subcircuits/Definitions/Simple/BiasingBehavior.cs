using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : SubcircuitBehavior<IBiasingBehavior>, IBiasingBehavior
    {
        private bool _localSolver = false;
        private IBiasingSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public BiasingBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            if (simulation.LocalConfiguration.TryGetValue(out BiasingParameters result))
            {
                if (result.LocalSolver && !simulation.LocalStates.ContainsKey(typeof(IBiasingSimulationState)))
                {
                    // Create a new state with a local solver
                    var state = simulation.GetState<IBiasingSimulationState>();
                    _state = new LocalBiasingSimulationState(
                        state,
                        LUHelper.CreateSparseRealSolver(),
                        new VariableMap(state.Map.Ground));
                    simulation.LocalStates.Add(_state);

                    // Once the behaviors are created, we'll want to deal with the sparse matrix order
                    simulation.AfterBehaviorCreation += ReorderLocalSolver;

                    _localSolver = true;
                }
                else
                    _state = simulation.GetState<IBiasingSimulationState>();
            }
        }

        /// <summary>
        /// Reorders the local solver.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ReorderLocalSolver(object sender, EventArgs e)
        {
            var solver = (SparseRealSolver<SparseMatrix<double>, SparseVector<double>>)_state.Solver;
            int sharedCount = 0;
            solver.Precondition((matrix, vector) =>
            {
                foreach (var variable in Simulation.SharedVariables)
                {
                    // Let's move this variable to the back
                    int index = _state.Map[variable];
                    var location = _state.Solver.ExternalToInternal(new MatrixLocation(index, index));
                    int target = matrix.Size - sharedCount;
                    matrix.SwapColumns(location.Column, target);
                    matrix.SwapRows(location.Row, target);
                    sharedCount++;
                }
            });
            solver.Order = _state.Solver.Size - sharedCount;
            solver.Strategy.SearchLimit = solver.Order;
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Load()
        {
            if (_localSolver)
                _state.Solver.Reset();
            foreach (var behavior in Behaviors)
                behavior.Load();
            if (_localSolver)
            {
                _state.Solver.OrderAndFactor();

                // Copy the elements to the parent solver
            }
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            // We need to fill up our local solution if necessary

            var result = true;
            foreach (var behavior in Behaviors)
                result &= behavior.IsConvergent();
            return result;
        }
    }
}
