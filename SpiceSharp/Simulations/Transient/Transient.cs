using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A time-domain analysis (Transient simulation)
    /// </summary>
    public class Transient : TimeSimulation
    {
        /// <summary>
        /// Event that is called when the timestep has been cut due to convergence problems
        /// </summary>
        public event EventHandler<TimestepCutEventArgs> TimestepCut;

        /// <summary>
        /// Behaviors for accepting a timepoint
        /// </summary>
        private BehaviorList<BaseAcceptBehavior> _acceptBehaviors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public Transient(Identifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="step">Step</param>
        /// <param name="final">Final time</param>
        public Transient(Identifier name, double step, double final) 
            : base(name, step, final)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="step">Step</param>
        /// <param name="final">Final time</param>
        /// <param name="maxStep">Maximum timestep</param>
        public Transient(Identifier name, double step, double final, double maxStep) 
            : base(name, step, final, maxStep)
        {
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        /// <param name="circuit">Circuit</param>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Get behaviors and configurations
            _acceptBehaviors = SetupBehaviors<BaseAcceptBehavior>(circuit.Objects);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (var i = 0; i < _acceptBehaviors.Count; i++)
                _acceptBehaviors[i].Unsetup();
            _acceptBehaviors = null;

            base.Unsetup();
        }

        /// <summary>
        /// Execute the transient simulation
        /// </summary>
        protected override void Execute()
        {
            // First do temperature-dependent calculations and IC
            base.Execute();
            var exportargs = new ExportDataEventArgs(this);

            var state = RealState;
            var baseConfig = BaseConfiguration;
            var timeConfig = TimeConfiguration;

            var delta = Math.Min(timeConfig.FinalTime / 50.0, timeConfig.Step) / 10.0;

            // Initialize before starting the simulation
            state.UseIc = timeConfig.UseIc;
            state.Domain = RealState.DomainType.Time;
            state.Gmin = baseConfig.Gmin;

            // Use node initial conditions if device initial conditions are not used
            if (!timeConfig.UseIc)
                AfterLoad += LoadInitialConditions;

            // Calculate the operating point
            Op(baseConfig.DcMaxIterations);
            Statistics.TimePoints++;
            Method.DeltaOld.Clear(timeConfig.MaxStep);
            Method.Delta = delta;
            Method.SaveDelta = timeConfig.FinalTime / 50.0;

            // Stop calculating a DC solution
            state.UseIc = false;
            state.UseDc = false;
            GetDcStates();
            AfterLoad -= LoadInitialConditions;

            // Start our statistics
            Statistics.TransientTime.Start();
            var startIters = Statistics.Iterations;
            var startselapsed = Statistics.SolveTime.Elapsed;

            try
            {
                while (true)
                {
                    // nextTime:

                    // Accept the current timepoint (CKTaccept())
                    for (var i = 0; i < _acceptBehaviors.Count; i++)
                        _acceptBehaviors[i].Accept(this);
                    Method.SaveSolution(state.Solution);
                    // end of CKTaccept()

                    // Check if current breakpoint is outdated; if so, clear
                    Method.UpdateBreakpoints();
                    Statistics.Accepted++;

                    // Export the current timepoint
                    if (Method.Time >= timeConfig.InitTime)
                    {
                        OnExport(exportargs);
                    }

                    // Detect the end of the simulation
                    if (Method.Time >= timeConfig.FinalTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TransientIterations += Statistics.Iterations - startIters;
                        Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Pause test - pausing not supported

                    // resume:
                    Method.Delta = Math.Min(Method.Delta, timeConfig.MaxStep);
                    Method.Resume();
                    StatePool.History.Cycle();

                    // Calculate a new solution
                    while (true)
                    {
                        Method.TryDelta();

                        // Compute coefficients and predict a solution and reset states to our previous solution
                        Method.ComputeCoefficients(this);
                        Method.Predict(this);

                        // Try to solve the new point
                        if (Method.SavedTime.Equals(0.0))
                            state.Init = RealState.InitializationStates.InitTransient;
                        var converged = TimeIterate(timeConfig.TranMaxIterations);
                        Statistics.TimePoints++;

                        // Spice copies the states the first time, we're not
                        // I believe this is because Spice treats the first timepoint after the OP as special (MODEINITTRAN)
                        // We don't treat it special (we just assume it started from a circuit in rest)

                        if (!converged)
                        {
                            // Failed to converge, let's try again with a smaller timestep
                            Method.Rollback();
                            Statistics.Rejected++;
                            Method.Delta /= 8.0;
                            Method.CutOrder();

                            var data = new TimestepCutEventArgs(Method.Delta / 8.0, TimestepCutEventArgs.TimestepCutReason.Convergence);
                            TimestepCut?.Invoke(this, data);
                        }
                        else
                        {
                            // Do not check the first time point
                            if (Method.SavedTime.Equals(0.0) || Method.LteControl(this))
                            {
                                // goto nextTime;
                                break;
                            }

                            Statistics.Rejected++;
                            var data = new TimestepCutEventArgs(Method.Delta, TimestepCutEventArgs.TimestepCutReason.Truncation);
                            TimestepCut?.Invoke(this, data);
                        }

                        if (Method.Delta <= timeConfig.DeltaMin)
                        {
                            if (Method.OldDelta > timeConfig.DeltaMin)
                                Method.Delta = timeConfig.DeltaMin;
                            else
                                throw new CircuitException("Timestep too small at t={0:e}: {1:e}".FormatString(Method.SavedTime, Method.Delta));
                        }
                    }
                }
            }
            catch (CircuitException ex)
            {
                // Keep our statistics
                Statistics.TransientTime.Stop();
                Statistics.TransientIterations += Statistics.Iterations - startIters;
                Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;
                throw new CircuitException("{0}: transient terminated".FormatString(Name), ex);
            }
        }

        /// <summary>
        /// Load initial conditions
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadInitialConditions(object sender, LoadStateEventArgs e)
        {
            var state = RealState;
            var nodes = Nodes;
            var solver = state.Solver;

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.InitialConditions.ContainsKey(node.Name))
                {
                    var ic = nodes.InitialConditions[node.Name];
                    if (ZeroNoncurrentRow(solver, nodes, node.Index))
                    {
                        // Avoid creating sparse elements if it is not necessary
                        if (!ic.Equals(0.0))
                            solver.GetRhsElement(node.Index).Value = 1.0e10 * ic;
                        node.Diagonal.Value = 1.0e10;
                    }
                    else
                    {
                        // Avoid creating sparse elements if it is not necessary
                        if (!ic.Equals(0.0))
                            solver.GetRhsElement(node.Index).Value = ic;
                        node.Diagonal.Value = 1.0;
                    }
                }
            }
        }
    }
}
