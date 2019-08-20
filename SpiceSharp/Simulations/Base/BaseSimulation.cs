using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

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
        /// Gets the currently active simulation state.
        /// </summary>
        public BaseSimulationState RealState { get; protected set; }
        
        /// <summary>
        /// Gets the variable that causes issues.
        /// </summary>
        /// <remarks>
        /// This variable can be used to close in on the problem in case of non-convergence.
        /// </remarks>
        public Variable ProblemVariable { get; protected set; }

        #region Events

        /// <summary>
        /// Occurs before loading the matrix and right-hand side vector.
        /// </summary>
        /// <remarks>
        /// For better performance, you can also create an entity with a high priority that
        /// generates a load behavior.
        /// </remarks>
        public event EventHandler<LoadStateEventArgs> BeforeLoad;

        /// <summary>
        /// Occurs after loading the matrix and right-hand side vector.
        /// </summary>
        /// <remarks>
        /// For better performance, you can also create an entity with a low priority that
        /// generates a load behavior.
        /// </remarks>
        public event EventHandler<LoadStateEventArgs> AfterLoad;

        /// <summary>
        /// Occurs before performing temperature-dependent calculations.
        /// </summary>
        /// <remarks>
        /// For better performance, you can also create an entity with a high priority that
        /// creates a temperature behavior.
        /// </remarks>
        public event EventHandler<LoadStateEventArgs> BeforeTemperature;

        /// <summary>
        /// Occurs after performing temperature-dependent calculations.
        /// </summary>
        /// <remarks>
        /// For better performance, you can also create an entity with a low priority that
        /// creates a temperature behavior.
        /// </remarks>
        public event EventHandler<LoadStateEventArgs> AfterTemperature;
        #endregion

        /// <summary>
        /// Private variables
        /// </summary>
        private LoadStateEventArgs _realStateLoadArgs;
        private BehaviorList<IBiasingBehavior> _loadBehaviors;
        private BehaviorList<ITemperatureBehavior> _temperatureBehaviors;
        private BehaviorList<IInitialConditionBehavior> _initialConditionBehaviors;
        private readonly List<ConvergenceAid> _nodesets = new List<ConvergenceAid>();
        private double _diagonalGmin;
        private bool _isPreordered, _shouldReorder;
        
        /// <summary>
        /// Gets the maximum number of allowed iterations for DC analysis.
        /// </summary>
        protected int DcMaxIterations { get; private set; }

        /// <summary>
        /// Gets the (cached) absolute tolerance on values to check convergence.
        /// </summary>
        protected double AbsTol { get; private set; }

        /// <summary>
        /// Gets the (cached) relative tolerance on values to check convergence.
        /// </summary>
        protected double RelTol { get; private set; }

        /// <summary>
        /// Gets the (cached) simulation statistics for the simulation.
        /// </summary>
        protected BaseSimulationStatistics BaseSimulationStatistics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected BaseSimulation(string name)
            : base(name)
        {
            Configurations.Add(new BaseConfiguration());
            BaseSimulationStatistics = new BaseSimulationStatistics();
            Statistics.Add(typeof(BaseSimulationStatistics), BaseSimulationStatistics);

            // Add the necessary behaviors in the order that they are (usually) called
            BehaviorTypes.AddRange(new []
            {
                typeof(ITemperatureBehavior),
                typeof(IBiasingBehavior),
                typeof(IInitialConditionBehavior)
            });
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        protected override void Setup(EntityCollection circuit)
        {
            circuit.ThrowIfNull(nameof(circuit));

            // Get behaviors and configuration data
            var config = Configurations.Get<BaseConfiguration>().ThrowIfNull("base configuration");
            DcMaxIterations = config.DcMaxIterations;
            AbsTol = config.AbsoluteTolerance;
            RelTol = config.RelativeTolerance;

            // Create the state for this simulation
            RealState = new BaseSimulationState
            {
                Gmin = config.Gmin
            };
            _isPreordered = false;
            _shouldReorder = true;
            var strategy = RealState.Solver.Strategy;
            strategy.RelativePivotThreshold = config.RelativePivotThreshold;
            strategy.AbsolutePivotThreshold = config.AbsolutePivotThreshold;

            // Setup the rest of the circuit.
            base.Setup(circuit);

            // Cache local variables
            _temperatureBehaviors = EntityBehaviors.GetBehaviorList<ITemperatureBehavior>();
            _loadBehaviors = EntityBehaviors.GetBehaviorList<IBiasingBehavior>();
            _initialConditionBehaviors = EntityBehaviors.GetBehaviorList<IInitialConditionBehavior>();
            _realStateLoadArgs = new LoadStateEventArgs(RealState);
            RealState.Setup(Variables);

            // Set up nodesets
            foreach (var ns in config.Nodesets)
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
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        protected void Temperature()
        {
            var args = new LoadStateEventArgs(RealState);
            OnBeforeTemperature(args);
            for (var i = 0; i < _temperatureBehaviors.Count; i++)
                _temperatureBehaviors[i].Temperature();
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
                _initialConditionBehaviors[i].Unbind();
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].Unbind();
            for (var i = 0; i < _temperatureBehaviors.Count; i++)
                _temperatureBehaviors[i].Unbind();

            // Clear the state
            RealState.Unsetup();
            RealState = null;
            _realStateLoadArgs = null;

            // Remove behavior and configuration references
            _loadBehaviors = null;
            _initialConditionBehaviors = null;
            _temperatureBehaviors = null;
        }

        /// <summary>
        /// Calculates the operating point of the circuit.
        /// </summary>
        /// <param name="maxIterations">The maximum amount of allowed iterations.</param>
        protected void Op(int maxIterations)
        {
            var state = RealState;
            var config = Configurations.Get<BaseConfiguration>().ThrowIfNull("base configuration");
            state.Init = InitializationModes.Junction;

            // First, let's try finding an operating point by using normal iterations
            if (!config.NoOperatingPointIterations)
            {
                if (Iterate(maxIterations))
                    return;
            }

            // Try Gmin stepping
            if (config.GminSteps > 1)
            {
                if (IterateGminStepping(maxIterations, config.GminSteps))
                    return;
            }

            // No convergence..., try GMIN stepping on the diagonal elements
            if (config.GminSteps > 1)
            {
                if (IterateDiagonalGminStepping(maxIterations, config.GminSteps))
                    return;
            }

            // Nope, still not converging, let's try source stepping
            if (config.SourceSteps > 1)
            {
                if (IterateSourceStepping(maxIterations, config.SourceSteps))
                    return;
            }

            // Failed
            throw new CircuitException("Could not determine operating point");
        }

        /// <summary>
        /// Iterates to a solution while shunting PN-junctions with a conductance.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations per step.</param>
        /// <param name="steps">The number of steps.</param>
        /// <returns></returns>
        protected bool IterateGminStepping(int maxIterations, int steps)
        {
            var state = RealState;

            // We will shunt all PN-junctions with a conductance (should be implemented by the components)
            CircuitWarning.Warning(this, Properties.Resources.StartGminStepping);

            // We could've ended up with some crazy value, so let's reset it
            for (var i = 0; i <= RealState.Solution.Length; i++)
                RealState.Solution[i] = 0.0;

            // Let's make it a bit easier for our iterations to converge
            var original = state.Gmin;
            if (state.Gmin <= 0)
                state.Gmin = 1e-12;
            for (var i = 0; i < steps; i++)
                state.Gmin *= 10.0;

            // Start GMIN stepping
            state.Init = InitializationModes.Junction;
            for (var i = 0; i <= steps; i++)
            {
                state.IsConvergent = false;
                if (!Iterate(maxIterations))
                {
                    state.Gmin = original;
                    CircuitWarning.Warning(this, Properties.Resources.GminSteppingFailed);
                    break;
                }

                // Success! Let's try to get more accurate now
                state.Gmin /= 10.0;
                state.Init = InitializationModes.Float;
            }

            // Try one more time with the original gmin
            state.Gmin = original;
            return Iterate(maxIterations);
        }

        /// <summary>
        /// Iterates to a solution while adding a conductive path to ground on all nodes.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations per step.</param>
        /// <param name="steps">The number of steps.</param>
        /// <returns></returns>
        protected bool IterateDiagonalGminStepping(int maxIterations, int steps)
        {
            var state = RealState;

            // We will add a DC path to ground to all nodes to aid convergence
            CircuitWarning.Warning(this, Properties.Resources.StartDiagonalGminStepping);

            // We'll hack into the loading algorithm to apply our diagonal contributions
            _diagonalGmin = state.Gmin;
            if (_diagonalGmin <= 0)
                _diagonalGmin = 1e-12;
            void ApplyGminStep(object sender, LoadStateEventArgs args) => state.Solver.ApplyDiagonalGmin(_diagonalGmin);
            AfterLoad += ApplyGminStep;

            // We could've ended up with some crazy value, so let's reset it
            for (var i = 0; i <= RealState.Solution.Length; i++)
                RealState.Solution[i] = 0.0;
                
            // Let's make it a bit easier for our iterations to converge
            for (var i = 0; i < steps; i++)
                _diagonalGmin *= 10.0;

            // Start GMIN stepping
            state.Init = InitializationModes.Junction;
            for (var i = 0; i <= steps; i++)
            {
                state.IsConvergent = false;
                if (!Iterate(maxIterations))
                {
                    _diagonalGmin = 0.0;
                    CircuitWarning.Warning(this, Properties.Resources.GminSteppingFailed);
                    break;
                }
                _diagonalGmin /= 10.0;
                state.Init = InitializationModes.Float;
            }

            // Try one more time without the gmin stepping
            AfterLoad -= ApplyGminStep;
            _diagonalGmin = 0.0;
            return Iterate(maxIterations);
        }

        /// <summary>
        /// Iterates to a solution slowly ramping up independent voltages and currents.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations per step.</param>
        /// <param name="steps">The number of steps.</param>
        /// <returns></returns>
        protected bool IterateSourceStepping(int maxIterations, int steps)
        {
            var state = RealState;

            // We will slowly ramp up voltages starting at 0 to make sure it converges
            CircuitWarning.Warning(this, Properties.Resources.StartSourceStepping);

            // We could've ended up with some crazy value, so let's reset it
            for (var i = 0; i <= RealState.Solution.Length; i++)
                RealState.Solution[i] = 0.0;

            // Start SRC stepping
            bool success = true;
            state.Init = InitializationModes.Junction;
            for (var i = 0; i <= steps; i++)
            {
                state.SourceFactor = i / (double)steps;
                if (!Iterate(maxIterations))
                {
                    state.SourceFactor = 1.0;
                    CircuitWarning.Warning(this, Properties.Resources.SourceSteppingFailed);
                    success = false;
                    break;
                }
            }

            return success;
        }

        /// <summary>
        /// Iterates towards a solution.
        /// </summary>
        /// <param name="maxIterations">The maximum allowed iterations.</param>
        /// <returns>
        ///   <c>true</c> if the iterations converged to a solution; otherwise, <c>false</c>.
        /// </returns>
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
                    BaseSimulationStatistics.Iterations = iterno;
                    throw;
                }

                // Preorder matrix
                if (!_isPreordered)
                {
                    solver.PreorderModifiedNodalAnalysis(Math.Abs);
                    _isPreordered = true;
                }
                if (state.Init == InitializationModes.Junction)
                    _shouldReorder = true;

                // Reorder
                if (_shouldReorder)
                {
                    BaseSimulationStatistics.ReorderTime.Start();
                    solver.OrderAndFactor();
                    BaseSimulationStatistics.ReorderTime.Stop();
                    _shouldReorder = false;
                }
                else
                {
                    // Decompose
                    BaseSimulationStatistics.DecompositionTime.Start();
                    var success = solver.Factor();
                    BaseSimulationStatistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        _shouldReorder = true;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                state.StoreSolution();

                // Solve the equation
                BaseSimulationStatistics.SolveTime.Start();
                solver.Solve(state.Solution);
                BaseSimulationStatistics.SolveTime.Stop();

                // Reset ground nodes
                solver.GetRhsElement(0).Value = 0.0;
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    BaseSimulationStatistics.Iterations += iterno;
                    return false;
                }

                if (state.IsConvergent && iterno != 1)
                    state.IsConvergent = IsConvergent();
                else
                    state.IsConvergent = false;

                switch (state.Init)
                {
                    case InitializationModes.Float:
                        if (state.UseDc && _nodesets.Count > 0)
                        {
                            if (pass)
                                state.IsConvergent = false;
                            pass = false;
                        }
                        if (state.IsConvergent)
                        {
                            BaseSimulationStatistics.Iterations += iterno;
                            return true;
                        }
                        break;

                    case InitializationModes.Junction:
                        state.Init = InitializationModes.Fix;
                        _shouldReorder = true;
                        break;

                    case InitializationModes.Fix:
                        if (state.IsConvergent)
                            state.Init = InitializationModes.Float;
                        pass = true;
                        break;

                    case InitializationModes.None:
                        state.Init = InitializationModes.Float;
                        break;

                    default:
                        BaseSimulationStatistics.Iterations += iterno;
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
            BaseSimulationStatistics.LoadTime.Start();
            OnBeforeLoad(_realStateLoadArgs);

            // Clear rhs and matrix
            state.Solver.Clear();
            LoadBehaviors();

            // Keep statistics
            OnAfterLoad(_realStateLoadArgs);
            BaseSimulationStatistics.LoadTime.Stop();
        }

        /// <summary>
        /// Loads the current simulation state solver.
        /// </summary>
        protected virtual void LoadBehaviors()
        {
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].Load();
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
            if ((state.Init & (InitializationModes.Junction | InitializationModes.Fix)) ==
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
        protected bool IsConvergent()
        {
            var rstate = RealState;

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
                    var tol = RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + AbsTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
                else
                {
                    var tol = RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + AbsTol;
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
                if (!_loadBehaviors[i].IsConvergent())
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

        #if DEBUG
        /// <summary>
        /// Lists all variables to the debugger
        /// </summary>
        public void ListVariables()
        {
            foreach (var variable in Variables)
                System.Diagnostics.Debug.WriteLine(variable.Name + " (" + variable.Index + ") = " + RealState.Solution[variable.Index]);
        }
        #endif
    }
}
