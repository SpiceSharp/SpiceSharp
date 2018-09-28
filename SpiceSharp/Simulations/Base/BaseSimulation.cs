using System;
using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// The base simulation.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.Simulation" />
    /// <remarks>
    /// Pretty much all simulations start out with calculating the operating point of the circuit. So a <see cref="RealState" /> is always part of the simulation.
    /// </remarks>
    /// <seealso cref="Simulation" />
    public abstract class BaseSimulation : Simulation
    {
        /// <summary>
        /// Gets the currently active configuration.
        /// </summary>
        /// <value>
        /// The base configuration.
        /// </value>
        public BaseConfiguration BaseConfiguration { get; protected set; }

        /// <summary>
        /// Gets the currently active simulation state.
        /// </summary>
        /// <value>
        /// The real state.
        /// </value>
        public BaseSimulationState RealState { get; protected set; }

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public Statistics Statistics { get; } = new Statistics();

        /// <summary>
        /// Gets the variable that caused issues.
        /// </summary>
        /// <value>
        /// The problem variable.
        /// </value>
        /// <remarks>
        /// This variable can be used to close in on the problem in case of non-convergence.
        /// </remarks>
        public Variable ProblemVariable { get; protected set; }

        #region Events

        /// <summary>
        /// Occurs before loading the matrix and right-hand side vector.
        /// </summary>
        public event EventHandler<LoadStateEventArgs> BeforeLoad;

        /// <summary>
        /// Occurs after loading the matrix and right-hand side vector.
        /// </summary>
        public event EventHandler<LoadStateEventArgs> AfterLoad;

        /// <summary>
        /// Occurs before performing temperature-dependent calculations.
        /// </summary>
        public event EventHandler<LoadStateEventArgs> BeforeTemperature;

        /// <summary>
        /// Occurs after performing temperature-dependent calculations.
        /// </summary>
        public event EventHandler<LoadStateEventArgs> AfterTemperature;
        #endregion

        /// <summary>
        /// Private variables
        /// </summary>
        private LoadStateEventArgs _realStateLoadArgs;
        private BehaviorList<BaseLoadBehavior> _loadBehaviors;
        private BehaviorList<BaseTemperatureBehavior> _temperatureBehaviors;
        private BehaviorList<BaseInitialConditionBehavior> _initialConditionBehaviors;
        private List<ConvergenceAid> _nodesets = new List<ConvergenceAid>();
        private double _diagonalGmin = 0.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected BaseSimulation(Identifier name)
            : base(name)
        {
            ParameterSets.Add(new BaseConfiguration());
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Setup behaviors, configurations and states
            BaseConfiguration = ParameterSets.Get<BaseConfiguration>();
            _temperatureBehaviors = SetupBehaviors<BaseTemperatureBehavior>(circuit.Entities);
            _loadBehaviors = SetupBehaviors<BaseLoadBehavior>(circuit.Entities);
            _initialConditionBehaviors = SetupBehaviors<BaseInitialConditionBehavior>(circuit.Entities);

            // Create the state for this simulation
            RealState = new BaseSimulationState();

            // Setup the load behaviors
            _realStateLoadArgs = new LoadStateEventArgs(RealState);
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].GetEquationPointers(Variables, RealState.Solver);
            RealState.Setup(Variables);

            // TODO: Compatibility - nodesets from nodes instead of configuration should be removed eventually
            if (BaseConfiguration.Nodesets.Count == 0)
            {
                foreach (var ns in Variables.NodeSets)
                    _nodesets.Add(new ConvergenceAid(ns.Key, ns.Value));
            }

            // Set up nodesets
            foreach (var ns in BaseConfiguration.Nodesets)
                _nodesets.Add(new ConvergenceAid(ns.Key, ns.Value));
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            // Perform temperature-dependent calculations
            Temperature();

            // Apply nodesets if they are specified
            if (_nodesets.Count > 0)
            {
                // Initialize the nodesets
                foreach (var aid in _nodesets)
                    aid.Initialize(this);
                AfterLoad += LoadNodeSets;
            }

            // Copy configuration
            RealState.Gmin = BaseConfiguration.Gmin;
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        protected void Temperature()
        {
            var args = new LoadStateEventArgs(RealState);
            OnBeforeTemperature(args);
            for (var i = 0; i < _temperatureBehaviors.Count; i++)
                _temperatureBehaviors[i].Temperature(this);
            OnAfterTemperature(args);
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            base.Unsetup();

            // Remove nodeset
            AfterLoad -= LoadNodeSets;
            foreach (var aid in _nodesets)
                aid.Unsetup();
            _nodesets.Clear();

            // Unsetup all behaviors
            for (var i = 0; i < _initialConditionBehaviors.Count; i++)
                _initialConditionBehaviors[i].Unsetup(this);
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].Unsetup(this);
            for (var i = 0; i < _temperatureBehaviors.Count; i++)
                _temperatureBehaviors[i].Unsetup(this);

            // Clear the state
            RealState.Unsetup();
            RealState = null;
            _realStateLoadArgs = null;

            // Remove behavior and configuration references
            _loadBehaviors = null;
            _initialConditionBehaviors = null;
            _temperatureBehaviors = null;
            BaseConfiguration = null;
        }

        /// <summary>
        /// Calculates the operating point of the circuit.
        /// </summary>
        /// <param name="maxIterations">The maximum amount of allowed iterations.</param>
        /// <exception cref="SpiceSharp.CircuitException">Could not determine operating point</exception>
        protected void Op(int maxIterations)
        {
            var state = RealState;
            var config = BaseConfiguration;
            state.Init = BaseSimulationState.InitializationStates.InitJunction;

            // First, let's try finding an operating point by using normal iterations
            if (!config.NoOperatingPointIterations)
            {
                if (Iterate(maxIterations))
                {
                    return;
                }
            }

            // No convergence, try Gmin stepping
            if (config.GminSteps > 1)
            {
                // Apply gmin step to AfterLoad event
                _diagonalGmin = config.Gmin;
                void ApplyGminStep(object sender, LoadStateEventArgs args) => state.Solver.ApplyDiagonalGmin(_diagonalGmin);
                AfterLoad += ApplyGminStep;

                state.Init = BaseSimulationState.InitializationStates.InitJunction;
                CircuitWarning.Warning(this, Properties.Resources.StartGminStepping);
                for (var i = 0; i < config.GminSteps; i++)
                    _diagonalGmin *= 10.0;
                for (var i = 0; i <= config.GminSteps; i++)
                {
                    state.IsConvergent = false;
                    if (!Iterate(maxIterations))
                    {
                        _diagonalGmin = 0.0;
                        CircuitWarning.Warning(this, Properties.Resources.GminSteppingFailed);
                        break;
                    }
                    _diagonalGmin /= 10.0;
                    state.Init = BaseSimulationState.InitializationStates.InitFloat;
                }

                // Try one more time without the gmin stepping
                AfterLoad -= ApplyGminStep;
                _diagonalGmin = 0.0;
                if (Iterate(maxIterations))
                    return;
            }

            // Nope, still not converging, let's try source stepping
            if (config.SourceSteps > 1)
            {
                state.Init = BaseSimulationState.InitializationStates.InitJunction;
                CircuitWarning.Warning(this, Properties.Resources.StartSourceStepping);
                for (var i = 0; i <= config.SourceSteps; i++)
                {
                    state.SourceFactor = i / (double)config.SourceSteps;
                    if (!Iterate(maxIterations))
                    {
                        state.SourceFactor = 1.0;
                        CircuitWarning.Warning(this, Properties.Resources.SourceSteppingFailed);
                        return;
                    }
                }
                state.SourceFactor = 1.0;
                return;
            }

            // Failed
            throw new CircuitException("Could not determine operating point");
        }

        /// <summary>
        /// Iterates towards a solution.
        /// </summary>
        /// <param name="maxIterations">The maximum allowed iterations.</param>
        /// <returns>
        ///   <c>true</c> if the iterations converged to a solution; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="SpiceSharp.CircuitException">Could not find flag</exception>
        protected bool Iterate(int maxIterations)
        {
            var state = RealState;
            var solver = state.Solver;
            var pass = false;
            var iterno = 0;

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIc)
            {
                state.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load();
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                state.IsConvergent = true;

                try
                {
                    Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    Statistics.Iterations = iterno;
                    throw;
                }

                // Preorder matrix
                if ((state.Sparse & BaseSimulationState.SparseStates.DidPreorder) == 0)
                {
                    solver.PreorderModifiedNodalAnalysis(Math.Abs);
                    state.Sparse |= BaseSimulationState.SparseStates.DidPreorder;
                }
                if (state.Init == BaseSimulationState.InitializationStates.InitJunction)
                {
                    state.Sparse |= BaseSimulationState.SparseStates.ShouldReorder;
                }

                // Reorder
                if ((state.Sparse & BaseSimulationState.SparseStates.ShouldReorder) != 0) // state.Sparse.HasFlag(RealState.SparseStates.ShouldReorder)
                {
                    Statistics.ReorderTime.Start();
                    solver.OrderAndFactor();
                    Statistics.ReorderTime.Stop();
                    state.Sparse &= ~BaseSimulationState.SparseStates.ShouldReorder;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    var success = solver.Factor();
                    Statistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        state.Sparse |= BaseSimulationState.SparseStates.ShouldReorder;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                state.StoreSolution();

                // Solve the equation
                Statistics.SolveTime.Start();
                solver.Solve(state.Solution);
                Statistics.SolveTime.Stop();

                // Reset ground nodes
                solver.GetRhsElement(0).Value = 0.0;
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    Statistics.Iterations += iterno;
                    return false;
                }

                if (state.IsConvergent && iterno != 1)
                    state.IsConvergent = IsConvergent();
                else
                    state.IsConvergent = false;

                switch (state.Init)
                {
                    case BaseSimulationState.InitializationStates.InitFloat:
                        if (state.UseDc && state.HadNodeSet)
                        {
                            if (pass)
                                state.IsConvergent = false;
                            pass = false;
                        }
                        if (state.IsConvergent)
                        {
                            Statistics.Iterations += iterno;
                            return true;
                        }
                        break;

                    case BaseSimulationState.InitializationStates.InitJunction:
                        state.Init = BaseSimulationState.InitializationStates.InitFix;
                        state.Sparse |= BaseSimulationState.SparseStates.ShouldReorder;
                        break;

                    case BaseSimulationState.InitializationStates.InitFix:
                        if (state.IsConvergent)
                            state.Init = BaseSimulationState.InitializationStates.InitFloat;
                        pass = true;
                        break;

                    case BaseSimulationState.InitializationStates.None:
                        state.Init = BaseSimulationState.InitializationStates.InitFloat;
                        break;

                    default:
                        Statistics.Iterations += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }

        /// <summary>
        /// Load the current simulation state solver.
        /// </summary>
        protected void Load()
        {
            var state = RealState;

            // Start the stopwatch
            Statistics.LoadTime.Start();
            OnBeforeLoad(_realStateLoadArgs);

            // Clear rhs and matrix
            state.Solver.Clear();
            LoadBehaviors();

            // Keep statistics
            OnAfterLoad(_realStateLoadArgs);
            Statistics.LoadTime.Stop();
        }

        /// <summary>
        /// Loads the current simulation state solver.
        /// </summary>
        protected virtual void LoadBehaviors()
        {
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].Load(this);
        }

        /// <summary>
        /// Applies nodesets.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadNodeSets(object sender, LoadStateEventArgs e)
        {
            var state = RealState;

            // Consider doing nodeset assignments when we're starting out or in trouble
            if ((state.Init & (BaseSimulationState.InitializationStates.InitJunction | BaseSimulationState.InitializationStates.InitFix)) ==
                0) 
                return;

            // Aid in convergence
            foreach (var aid in _nodesets)
                aid.Aid();
        }

        /// <summary>
        /// Checks that the solution converges to a solution.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the solution converges; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="SpiceSharp.CircuitException">Non-convergence, node {0} is not a number.".FormatString(node)</exception>
        protected bool IsConvergent()
        {
            var rstate = RealState;
            var config = BaseConfiguration;

            // Check convergence for each node
            for (var i = 0; i < Variables.Count; i++)
            {
                var node = Variables[i];
                var n = rstate.Solution[node.Index];
                var o = rstate.OldSolution[node.Index];

                if (double.IsNaN(n))
                    throw new CircuitException("Non-convergence, node {0} is not a number.".FormatString(node));

                if (node.UnknownType == VariableType.Voltage)
                {
                    var tol = config.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + config.VoltageTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
                else
                {
                    var tol = config.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + config.AbsoluteTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
            }

            // Device-level convergence tests
            for (var i = 0; i < _loadBehaviors.Count; i++)
            {
                if (!_loadBehaviors[i].IsConvergent(this))
                {
                    // I believe this should be false, but Spice 3f5 doesn't...

                    /*
                     * Each device that checks convergence returns (OK) = 0 regardless
                     * of convergence (eg. Dev2/mos2conv.c). Not being convergent is communicated
                     * through the CKTnoncon variable (state.IsCon for Spice#).
                     * 
                     * The convergence methods are called in CKT/cktop.c at line 121. If an error
                     * occurs, it is returned. If non-convergence is detected through CKTnoncon,
                     * (OK) is returned anyway, so it doesn't make a difference. Remember that 
                     * our devices aren't returning anything else but (OK) anyway.
                     * 
                     * CKTconvTest in turn is called in NI/niconv.c at line 65. The result is
                     * therefore always (OK) too, and so the returned value by NIconvTest()
                     * is always (OK) if each device has tested convergence. Note that 1 is
                     * returned in the case of non-convergence for nodes!
                     * 
                     * Finally, in NI/niiter.c at line 184, when convergence is tested, the result
                     * is used to overwrite CKTnoncon, so there is no way we can still find out if
                     * any device detected non-convergence.
                     */
                    return true;
                }
            }

            // Convergence succeeded
            return true;
        }

        #region Methods for calling events

        /// <summary>
        /// Raises the <see cref="E:BeforeLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeLoad(LoadStateEventArgs args) => BeforeLoad?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:AfterLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterLoad(LoadStateEventArgs args) => AfterLoad?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:BeforeTemperature" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeTemperature(LoadStateEventArgs args) => BeforeTemperature?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:AfterTemperature" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterTemperature(LoadStateEventArgs args) => AfterTemperature?.Invoke(this, args);

        #endregion
    }
}
