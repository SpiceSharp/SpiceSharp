using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations.Biasing;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that can bias components.
    /// </summary>
    /// <seealso cref="Simulation" />
    /// <seealso cref="IBiasingSimulation"/>
    /// <seealso cref="IStateful{S}"/>
    /// <seealso cref="TemperatureSimulationState"/>
    /// <seealso cref="IIterationSimulationState"/>
    /// <seealso cref="IBiasingUpdateBehavior"/>
    /// <seealso cref="IBehavioral{T}" />
    /// <seealso cref="IParameterized{P}"/>
    public abstract partial class BiasingSimulation : Simulation,
        IBiasingSimulation, IStateful<TemperatureSimulationState>,
        IStateful<IIterationSimulationState>,
        IBehavioral<IBiasingUpdateBehavior>,
        IParameterized<BiasingParameters>
    {
        private LoadStateEventArgs _realStateLoadArgs;
        private BehaviorList<IBiasingBehavior> _loadBehaviors;
        private BehaviorList<IConvergenceBehavior> _convergenceBehaviors;
        private BehaviorList<IBiasingUpdateBehavior> _updateBehaviors;
        private BehaviorList<ITemperatureBehavior> _temperatureBehaviors;
        private readonly List<ConvergenceAid> _nodesets = [];
        private bool _isPreordered, _shouldReorder;
        private SimulationState _state;
        private TemperatureSimulationState _temperature;

        /// <summary>
        /// Represents the action before doing temperature-dependent calculations.
        /// </summary>
        public const int BeforeTemperature = 0x0100_0000;

        /// <summary>
        /// Represents the action after doing temperature-dependent calculations.
        /// </summary>
        public const int AfterTemperature = 0x0200_0000;

        /// <summary>
        /// Gets the variable that causes issues.
        /// </summary>
        /// <remarks>
        /// This variable can be used to close in on the problem in case of non-convergence.
        /// </remarks>
        public IVariable ProblemVariable { get; protected set; }

        /// <summary>
        /// Gets the biasing parameters.
        /// </summary>
        /// <value>
        /// The biasing parameters.
        /// </value>
        public BiasingParameters BiasingParameters { get; } = new BiasingParameters();

        /// <summary>
        /// Gets the iteration state.
        /// </summary>
        /// <value>
        /// The iteration state.
        /// </value>
        protected IterationState Iteration { get; } = new IterationState();

        /// <inheritdoc />
        IIterationSimulationState IStateful<IIterationSimulationState>.State => Iteration;

        /// <inheritdoc />
        ITemperatureSimulationState IStateful<ITemperatureSimulationState>.State => _temperature;

        /// <inheritdoc />
        IBiasingSimulationState IStateful<IBiasingSimulationState>.State => _state;

        /// <inheritdoc />
        TemperatureSimulationState IStateful<TemperatureSimulationState>.State => _temperature;

        /// <inheritdoc />
        BiasingParameters IParameterized<BiasingParameters>.Parameters => BiasingParameters;

        /// <inheritdoc />
        IVariableDictionary<IVariable<double>> ISimulation<IVariable<double>>.Solved => _state;

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
        #endregion

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public new BiasingSimulationStatistics Statistics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingSimulation"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected BiasingSimulation(string name)
            : base(name)
        {
            ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
            Statistics = new BiasingSimulationStatistics();
        }

        /// <inheritdoc/>
        protected override void CreateStates()
        {
            // Create simulation states
            _temperature = new TemperatureSimulationState(BiasingParameters.Temperature, BiasingParameters.NominalTemperature);
            _state = new SimulationState(BiasingParameters.CreateSolver(), BiasingParameters.NodeComparer);
            Iteration.Gmin = BiasingParameters.Gmin;
            _isPreordered = false;
            _shouldReorder = true;
            _realStateLoadArgs = new LoadStateEventArgs(_state);
        }

        /// <inheritdoc/>
        protected override void CreateBehaviors(IEntityCollection entities)
        {
            base.CreateBehaviors(entities);
            _temperatureBehaviors = EntityBehaviors.GetBehaviorList<ITemperatureBehavior>();
            _loadBehaviors = EntityBehaviors.GetBehaviorList<IBiasingBehavior>();
            _convergenceBehaviors = EntityBehaviors.GetBehaviorList<IConvergenceBehavior>();
            _updateBehaviors = EntityBehaviors.GetBehaviorList<IBiasingUpdateBehavior>();

            // We're going to set up our state now that all behaviors have allocated elements in the solver
            _state.Setup();

            // Set up nodesets for nodes that were referenced by an entity
            _nodesets.Clear();
            foreach (var ns in BiasingParameters.Nodesets)
            {
                if (_state.TryGetValue(ns.Key, out var variable))
                    _nodesets.Add(new ConvergenceAid(variable, _state, ns.Value));
                else
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_ConvergenceAidVariableNotFound.FormatString(ns.Key));
            }
        }

        /// <inheritdoc/>
        protected override void Validate(IEntityCollection entities)
        {
            if (BiasingParameters.Validate)
                Validate(new Rules(_state, BiasingParameters.NodeComparer), entities);
        }

        /// <inheritdoc/>
        protected override IEnumerable<int> Execute(int mask)
        {
            if ((mask & BeforeTemperature) != 0)
                yield return BeforeTemperature;

            // Perform temperature-dependent calculations
            foreach (var behavior in _temperatureBehaviors)
                behavior.Temperature();

            if ((mask & AfterTemperature) != 0)
                yield return AfterTemperature;

            // Apply nodesets if they are specified
            if (_nodesets.Count > 0)
                AfterLoad += LoadNodeSets;
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            // Remove nodeset events
            AfterLoad -= LoadNodeSets;
        }

        /// <summary>
        /// Calculates the operating point of the circuit.
        /// </summary>
        /// <param name="maxIterations">The maximum amount of allowed iterations.</param>
        protected void Op(int maxIterations)
        {
            Iteration.Mode = IterationModes.Junction;

            // First, let's try finding an operating point by using normal iterations
            if (!BiasingParameters.NoOperatingPointIterate)
            {
                if (Iterate(maxIterations))
                    return;
            }

            // Try Gmin stepping
            if (BiasingParameters.GminSteps > 1)
            {
                if (IterateGminStepping(maxIterations, BiasingParameters.GminSteps))
                    return;
            }

            // No convergence..., try GMIN stepping on the diagonal elements
            if (BiasingParameters.GminSteps > 1)
            {
                if (IterateDiagonalGminStepping(maxIterations, BiasingParameters.GminSteps))
                    return;
            }

            // Nope, still not converging, let's try source stepping
            if (BiasingParameters.SourceSteps > 1)
            {
                if (IterateSourceStepping(maxIterations, BiasingParameters.SourceSteps))
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
        /// <returns>
        /// <c>true</c> if the gmin stepping succeeded; otherwise <c>false</c>.
        /// </returns>
        protected bool IterateGminStepping(int maxIterations, int steps)
        {
            // We will shunt all PN-junctions with a conductance (should be implemented by the components)
            SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_StartGminStepping);

            // We could've ended up with some crazy value, so let's reset it
            for (int i = 0; i <= _state.Solution.Length; i++)
                _state.Solution[i] = 0.0;

            // Let's make it a bit easier for our iterations to converge
            double original = Iteration.Gmin;
            if (Iteration.Gmin <= 0)
                Iteration.Gmin = 1e-12;
            for (int i = 0; i < steps; i++)
                Iteration.Gmin *= 10.0;

            // Start GMIN stepping
            Iteration.Mode = IterationModes.Junction;
            for (int i = 0; i <= steps; i++)
            {
                Iteration.IsConvergent = false;
                if (!Iterate(maxIterations))
                {
                    Iteration.Gmin = original;
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulation_Biasing_GminSteppingFailed);
                    break;
                }

                // Success! Let's try to get more accurate now
                Iteration.Gmin /= 10.0;
                Iteration.Mode = IterationModes.Float;
            }

            // Try one more time with the original gmin
            Iteration.Gmin = original;
            return Iterate(maxIterations);
        }

        /// <summary>
        /// Iterates to a solution while adding a conductive path to ground on all nodes.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations per step.</param>
        /// <param name="steps">The number of steps.</param>
        /// <returns>
        /// <c>true</c> if the diagonal gmin stepping succeeded; otherwise <c>false</c>.
        /// </returns>
        protected bool IterateDiagonalGminStepping(int maxIterations, int steps)
        {
            // We will add a DC path to ground to all nodes to aid convergence
            SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_StartDiagonalGminStepping);

            // We'll hack into the loading algorithm to apply our diagonal contributions
            double diagonalGmin = Math.Min(Iteration.Gmin, 1e-12);
            void ApplyGminStep(object sender, LoadStateEventArgs args)
                => _state.Solver.Precondition((matrix, vector) => ModifiedNodalAnalysisHelper<double>.ApplyDiagonalGmin(matrix, diagonalGmin));
            AfterLoad += ApplyGminStep;

            // We could've ended up with some crazy value, so let's reset it
            for (int i = 0; i <= _state.Solution.Length; i++)
                _state.Solution[i] = 0.0;

            // Let's make it a bit easier for our iterations to converge
            for (int i = 0; i < steps; i++)
                diagonalGmin *= 10.0;

            // Start GMIN stepping
            Iteration.Mode = IterationModes.Junction;
            for (int i = 0; i <= steps; i++)
            {
                Iteration.IsConvergent = false;
                if (!Iterate(maxIterations))
                {
                    diagonalGmin = 0.0;
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulation_Biasing_GminSteppingFailed);
                    break;
                }
                diagonalGmin /= 10.0;
                Iteration.Mode = IterationModes.Float;
            }

            // Try one more time without the gmin stepping
            AfterLoad -= ApplyGminStep;
            diagonalGmin = 0.0;
            return Iterate(maxIterations);
        }

        /// <summary>
        /// Iterates to a solution slowly ramping up independent voltages and currents.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations per step.</param>
        /// <param name="steps">The number of steps.</param>
        /// <returns>
        /// <c>true</c> if source stepping succeeded; otherwise <c>false</c>.
        /// </returns>
        protected bool IterateSourceStepping(int maxIterations, int steps)
        {
            // We will slowly ramp up voltages starting at 0 to make sure it converges
            SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_Biasing_StartSourceStepping);

            // We could've ended up with some crazy value, so let's reset it
            for (int i = 0; i <= _state.Solution.Length; i++)
                _state.Solution[i] = 0.0;

            // Start SRC stepping
            bool success = true;
            Iteration.Mode = IterationModes.Junction;
            for (int i = 0; i <= steps; i++)
            {
                Iteration.SourceFactor = i / (double)steps;
                if (!Iterate(maxIterations))
                {
                    Iteration.SourceFactor = 1.0;
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
        /// <exception cref="SpiceSharpException">Thrown if any behavior cannot load the matrix and/or right hand side vector, or if the solution
        /// is not a number (NaN).</exception>
        /// <exception cref="SingularException">Thrown if the equation matrix is singular.</exception>
        protected virtual bool Iterate(int maxIterations)
        {
            var solver = _state.Solver;
            bool pass = false;
            int iterno = 0;

            try
            {
                while (true)
                {
                    // Load the Y-matrix and right hand side vector
                    Iteration.IsConvergent = true;
                    Load();
                    iterno++;

                    // Preorder matrix
                    if (!_isPreordered)
                    {
                        solver.Precondition((matrix, vector)
                            => ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix, solver.Size - solver.Degeneracy));
                        _isPreordered = true;
                    }
                    if (Iteration.Mode == IterationModes.Junction)
                        _shouldReorder = true;

                    if (_shouldReorder)
                    {
                        // Reorder and factor the solver
                        Statistics.ReorderTime.Start();
                        try
                        {
                            int eliminated = solver.OrderAndFactor();
                            if (eliminated < solver.Size)
                            {
                                // We should avoid throwing an exception here, because this may just be a starting
                                // error...
                                SpiceSharpWarning.Warning(this, Properties.Resources.Algebra_SingularMatrixIndexed.FormatString(eliminated + 1));
                                return false;
                            }
                            _shouldReorder = false;
                        }
                        finally
                        {
                            Statistics.ReorderTime.Stop();
                        }
                    }
                    else
                    {
                        // Just factor the solver (without losing time reordering)
                        Statistics.FactoringTime.Start();
                        try
                        {
                            if (!solver.Factor())
                            {
                                _shouldReorder = true;
                                continue;
                            }
                        }
                        finally
                        {
                            Statistics.FactoringTime.Stop();
                        }
                    }

                    // The current solution becomes the old solution
                    _state.StoreSolution();

                    // Solve the equation
                    Statistics.SolveTime.Start();
                    try
                    {
                        solver.ForwardSubstitute(_state.Solution);
                        solver.BackwardSubstitute(_state.Solution);
                    }
                    finally
                    {
                        Statistics.SolveTime.Stop();
                    }

                    // Reset ground nodes
                    solver.GetElement(0).Value = 0.0;
                    _state.Solution[0] = 0.0;
                    _state.OldSolution[0] = 0.0;

                    // Update 
                    foreach (var behavior in _updateBehaviors)
                        behavior.Update();

                    // Exceeded maximum number of iterations
                    if (iterno > maxIterations)
                        return false;

                    if (Iteration.IsConvergent && iterno != 1)
                        Iteration.IsConvergent = IsConvergent();
                    else
                        Iteration.IsConvergent = false;

                    switch (Iteration.Mode)
                    {
                        case IterationModes.Float:
                            if (_nodesets.Count > 0)
                            {
                                if (pass)
                                    Iteration.IsConvergent = false;
                                pass = false;
                            }
                            if (Iteration.IsConvergent)
                                return true;
                            break;

                        case IterationModes.Junction:
                            Iteration.Mode = IterationModes.Fix;
                            _shouldReorder = true;
                            break;

                        case IterationModes.Fix:
                            if (Iteration.IsConvergent)
                                Iteration.Mode = IterationModes.Float;
                            pass = true;
                            break;

                        case IterationModes.None:
                            Iteration.Mode = IterationModes.Float;
                            break;

                        default:
                            throw new SpiceSharpException(Properties.Resources.Simulations_InvalidInitializationMode);
                    }
                }
            }
            finally
            {
                Statistics.Iterations += iterno;
            }
        }

        /// <summary>
        /// Stores the solution.
        /// </summary>
        protected void StoreSolution() => _state.StoreSolution();

        /// <summary>
        /// Load the current simulation state solver.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown if any behavior cannot load the matrix or right hand side vector.</exception>
        protected void Load()
        {
            OnBeforeLoad(_realStateLoadArgs);

            Statistics.LoadTime.Start();
            try
            {
                // Clear rhs and matrix
                _state.Solver.Reset();
                foreach (var behavior in _loadBehaviors)
                    behavior.Load();
            }
            finally
            {
                Statistics.LoadTime.Stop();
            }

            OnAfterLoad(_realStateLoadArgs);
        }

        /// <summary>
        /// Applies nodesets.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="e">The event arguments for loading the convergence aids.</param>
        protected void LoadNodeSets(object sender, LoadStateEventArgs e)
        {
            // Consider doing nodeset assignments when we're starting out or in trouble
            if (Iteration.Mode != IterationModes.Junction && Iteration.Mode != IterationModes.Fix)
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
        /// <exception cref="SpiceSharpException">Thrown if a solution is not a number (NaN).</exception>
        protected bool IsConvergent()
        {
            // Check convergence for each node
            foreach (var v in _state.Map)
            {
                var node = v.Key;
                double n = _state.Solution[v.Value];
                double o = _state.OldSolution[v.Value];

                if (double.IsNaN(n))
                    throw new SpiceSharpException(Properties.Resources.Simulation_VariableNotANumber.FormatString(v));

                if (node.Unit == Units.Volt)
                {
                    double tol = BiasingParameters.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + BiasingParameters.VoltageTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
                else
                {
                    double tol = BiasingParameters.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + BiasingParameters.AbsoluteTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
            }

            // Device-level convergence tests
            foreach (var behavior in _convergenceBehaviors)
            {
                if (!behavior.IsConvergent())
                {
                    // I believe this should return false, but Spice 3f5 and ngSpice don't...

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
        #endregion
    }
}
