using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Biasing state for <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBiasingState" />
    /// <seealso cref="IBiasingSimulationState" />
    public class SolveBiasingState : SubcircuitBiasingState, IBiasingSimulationState
    {
        private IBiasingSimulationState _parent;
        private Dictionary<Element<double>, Element<double>> _elements = new Dictionary<Element<double>, Element<double>>();
        private Dictionary<int, int> _commonIndices = new Dictionary<int, int>();

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
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        public double SourceFactor => _parent.SourceFactor;

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        public double Gmin => _parent.Gmin;

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        public bool IsConvergent 
        {
            get => _parent.IsConvergent && _isConvergent;
            set => _isConvergent = true; 
        }
        private bool _isConvergent;

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
        /// Gets the solution vector.
        /// </summary>
        public IVector<double> Solution { get; private set; }

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        public IVector<double> OldSolution { get; private set; }

        /// <summary>
        /// Gets the sparse solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISparseSolver<double> Solver => _solver;
        private SparseLUSolver<SparseMatrix<double>, SparseVector<double>, double> _solver;

        /// <summary>
        /// Gets the variable to index map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolveBiasingState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public SolveBiasingState(IBiasingSimulationState parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _solver = LUHelper.CreateSparseRealSolver();
            Map = new VariableMap(parent.Map.Ground);
        }

        /// <summary>
        /// Notifies the state that these variables can be shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public override void ShareVariables(HashSet<Variable> common)
        {
            _solver.Order = 0;
            _solver.Strategy.SearchLimit = 0;
            int target = Solver.Size;

            // We need to move any shared variables to the end of the solver
            var local_indices = new int[common.Count];
            var global_indices = new int[common.Count];
            int count = 0;
            foreach (var node in common)
            {
                if (node == Map.Ground)
                    continue;

                // Only apply for nodes that this state is using
                if (!Map.TryGetIndex(node, out var local_index))
                    continue;
                if (!_parent.Map.TryGetIndex(node, out var global_index))
                    global_index = _parent.Map[node];
                global_indices[count] = global_index;
                local_indices[count++] = local_index;
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
            _solver.Order = target;
            _solver.Strategy.SearchLimit = target;

            // Map all solver elements
            for (var i = 0; i < count; i++)
            {
                if (global_indices[i] == 0)
                    continue;
                var local_elt = Solver.GetElement(local_indices[i]);
                var global_elt = _parent.Solver.GetElement(global_indices[i]);
                _elements.Add(local_elt, global_elt);
                for (var j = 0; j <= count; j++)
                {
                    if (global_indices[j] == 0)
                        continue;
                    local_elt = Solver.GetElement(local_indices[i], local_indices[j]);
                    global_elt = _parent.Solver.GetElement(global_indices[i], global_indices[j]);
                    _elements.Add(local_elt, global_elt);
                }
            }
        }

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        public override void ApplyAsynchroneously()
        {
            // Do a partial solve of the solver
            Solver.OrderAndFactor();
        }

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public override void ApplySynchroneously()
        {
            // Add the contributions to the parent solver
            foreach (var pairs in _elements)
                pairs.Value.Add(pairs.Key.Value);
        }

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public override void Reset()
        {
            // Swap current and old solution
            var tmp = Solution;
            Solution = OldSolution;
            OldSolution = tmp;

            // Finally reset the solver
            Solver.Reset();
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
                Solution[pair.Key] = _parent.Solution[pair.Value];

            // Solve to our local solution for the other elements
            Solver.Solve(Solution);

            // Check for convergence on the variables, similar to BiasingSimulation.
            return true;
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Setup(ISimulation simulation)
        {
            Solution = new DenseVector<double>(Solver.Size);
            OldSolution = new DenseVector<double>(Solver.Size);
            Temperature = _parent.Temperature;
        }

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        public void Unsetup()
        {
            Solution = null;
            OldSolution = null;
        }
    }
}
