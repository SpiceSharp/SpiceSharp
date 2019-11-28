using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can bias components.
    /// </summary>
    /// <seealso cref="Simulation" />
    /// <seealso cref="IBiasingSimulation"/>
    /// <seealso cref="IBehavioral{T}" />
    public abstract partial class BiasingSimulation : Simulation,
        IBiasingSimulation,
        IBehavioral<IBiasingUpdateBehavior>
    {
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
        private BehaviorList<IBiasingUpdateBehavior> _updateBehaviors;
        private BehaviorList<ITemperatureBehavior> _temperatureBehaviors;
        private readonly List<ConvergenceAid> _nodesets = new List<ConvergenceAid>();
        private double _diagonalGmin;
        private bool _isPreordered, _shouldReorder;

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected BiasingSimulationState BiasingState { get; private set; }
        
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
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public new BiasingSimulationStatistics Statistics { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public IBiasingSimulationState State => BiasingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected BiasingSimulation(string name)
            : base(name)
        {
            Configurations.Add(new BiasingConfiguration());
            Statistics = new BiasingSimulationStatistics();
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        protected override void Setup(IEntityCollection circuit)
        {
            circuit.ThrowIfNull(nameof(circuit));

            // Get behaviors and configuration data
            var config = Configurations.GetValue<BiasingConfiguration>();
            DcMaxIterations = config.DcMaxIterations;
            AbsTol = config.AbsoluteTolerance;
            RelTol = config.RelativeTolerance;

            // Setup the rest of the circuit.
            base.Setup(circuit);

            // Cache local variables
            _temperatureBehaviors = EntityBehaviors.GetBehaviorList<ITemperatureBehavior>();
            _loadBehaviors = EntityBehaviors.GetBehaviorList<IBiasingBehavior>();
            _updateBehaviors = EntityBehaviors.GetBehaviorList<IBiasingUpdateBehavior>();
            _realStateLoadArgs = new LoadStateEventArgs(BiasingState);
            BiasingState.Setup(this);

            // Set up nodesets
            foreach (var ns in config.Nodesets)
                _nodesets.Add(new ConvergenceAid(ns.Key, ns.Value));
        }

        /// <summary>
        /// Creates all behaviors for the simulation.
        /// </summary>
        /// <param name="entities">The entities.</param>
        protected override void CreateBehaviors(IEntityCollection entities)
        {
            var config = Configurations.GetValue<BiasingConfiguration>();

            // Create the state for this simulation
            ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
            BiasingState = new BiasingSimulationState(
                config.Solver ?? Algebra.LUHelper.CreateSparseRealSolver(),
                config.Map ?? new VariableMap(Variables.Ground));
            BiasingState.Gmin = config.Gmin;
            _isPreordered = false;
            _shouldReorder = true;
            /* var strategy = _state.Solver.Strategy;
            strategy.RelativePivotThreshold = config.RelativePivotThreshold;
            strategy.AbsolutePivotThreshold = config.AbsolutePivotThreshold; */

            base.CreateBehaviors(entities);
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
            var args = new LoadStateEventArgs(BiasingState);
            OnBeforeTemperature(args);
            foreach (var behavior in _temperatureBehaviors)
                behavior.Temperature();
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
            _nodesets.Clear();

            _realStateLoadArgs = null;

            // Remove behavior and configuration references
            _loadBehaviors = null;
            _temperatureBehaviors = null;
        }

        /// <summary>
        /// Calculates the operating point of the circuit.
        /// </summary>
        /// <param name="maxIterations">The maximum amount of allowed iterations.</param>
        protected void Op(int maxIterations)
        {
            var state = BiasingState;
            var config = Configurations.GetValue<BiasingConfiguration>().ThrowIfNull("base configuration");
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
            throw new SpiceSharpException(Properties.Resources.Simulations_Biasing_NoOp);
        }

        /// <summary>
        /// Iterates to a solution while shunting PN-junctions with a conductance.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations per step.</param>
        /// <param name="steps">The number of steps.</param>
        /// <returns></returns>
        protected bool IterateGminStepping(int maxIterations, int steps)
        {
            var state = BiasingState;

            // We will shunt all PN-junctions with a conductance (should be implemented by the components)
            SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_StartGminStepping);

            // We could've ended up with some crazy value, so let's reset it
            for (var i = 0; i <= BiasingState.Solution.Length; i++)
                BiasingState.Solution[i] = 0.0;

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
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulation_Biasing_GminSteppingFailed);
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
            var state = BiasingState;

            // We will add a DC path to ground to all nodes to aid convergence
            SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_StartDiagonalGminStepping);

            // We'll hack into the loading algorithm to apply our diagonal contributions
            _diagonalGmin = state.Gmin;
            if (_diagonalGmin <= 0)
                _diagonalGmin = 1e-12;
            void ApplyGminStep(object sender, LoadStateEventArgs args)
                => BiasingState.Solver.Precondition((matrix, vector) => ModifiedNodalAnalysisHelper<double>.ApplyDiagonalGmin(matrix, _diagonalGmin));
            AfterLoad += ApplyGminStep;

            // We could've ended up with some crazy value, so let's reset it
            for (var i = 0; i <= BiasingState.Solution.Length; i++)
                BiasingState.Solution[i] = 0.0;
                
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
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulation_Biasing_GminSteppingFailed);
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
            var state = BiasingState;

            // We will slowly ramp up voltages starting at 0 to make sure it converges
            SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_StartSourceStepping);

            // We could've ended up with some crazy value, so let's reset it
            for (var i = 0; i <= BiasingState.Solution.Length; i++)
                BiasingState.Solution[i] = 0.0;

            // Start SRC stepping
            bool success = true;
            state.Init = InitializationModes.Junction;
            for (var i = 0; i <= steps; i++)
            {
                state.SourceFactor = i / (double)steps;
                if (!Iterate(maxIterations))
                {
                    state.SourceFactor = 1.0;
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_SourceSteppingFailed);
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
            var state = BiasingState;
            var solver = BiasingState.Solver;
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
                catch (SpiceSharpException)
                {
                    iterno++;
                    Statistics.Iterations = iterno;
                    throw;
                }

                // Preorder matrix
                if (!_isPreordered)
                {
                    solver.Precondition((matrix, vector)
                        => ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix, solver.Order));
                    _isPreordered = true;
                }
                if (state.Init == InitializationModes.Junction)
                    _shouldReorder = true;

                // Reorder
                if (_shouldReorder)
                {
                    Statistics.ReorderTime.Start();
                    solver.OrderAndFactor();
                    Statistics.ReorderTime.Stop();
                    _shouldReorder = false;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    var success = solver.Factor();
                    Statistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        _shouldReorder = true;
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
                solver.GetElement(0).Value = 0.0;
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                foreach (var behavior in _updateBehaviors)
                    behavior.Update();

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
                    case InitializationModes.Float:
                        if (state.UseDc && _nodesets.Count > 0)
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
                        Statistics.Iterations += iterno;
                        throw new SpiceSharpException(Properties.Resources.Simulations_InvalidInitializationMode);
                }
            }
        }

        /// <summary>
        /// Load the current simulation state solver.
        /// </summary>
        protected void Load()
        {
            // Start the stopwatch
            Statistics.LoadTime.Start();
            OnBeforeLoad(_realStateLoadArgs);

            // Clear rhs and matrix
            BiasingState.Solver.Reset();
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
            foreach (var behavior in _loadBehaviors)
                behavior.Load();
        }

        /// <summary>
        /// Applies nodesets.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadNodeSets(object sender, LoadStateEventArgs e)
        {
            var state = BiasingState;

            // Consider doing nodeset assignments when we're starting out or in trouble
            if ((state.Init & (InitializationModes.Junction | InitializationModes.Fix)) == 0) 
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
            var rstate = BiasingState;

            // Check convergence for each node
            foreach (var v in rstate.Map)
            {
                var node = v.Key;
                var n = rstate.Solution[v.Value];
                var o = rstate.OldSolution[v.Value];

                if (double.IsNaN(n))
                    throw new SpiceSharpException(Properties.Resources.Simulation_VariableNotANumber.FormatString(node));

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
            foreach (var behavior in _loadBehaviors)
            {
                if (!behavior.IsConvergent())
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
        /// Raises the <see cref="BeforeLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeLoad(LoadStateEventArgs args) => BeforeLoad?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterLoad(LoadStateEventArgs args) => AfterLoad?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="BeforeTemperature" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeTemperature(LoadStateEventArgs args) => BeforeTemperature?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterTemperature" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterTemperature(LoadStateEventArgs args) => AfterTemperature?.Invoke(this, args);

        #endregion
    }
}
