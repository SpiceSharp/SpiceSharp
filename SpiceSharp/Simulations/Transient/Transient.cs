using System;
using System.Collections.ObjectModel;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;

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
        protected Collection<AcceptBehavior> AcceptBehaviors { get; private set; }

        /// <summary>
        /// Behaviors for truncating the timestep
        /// </summary>
        protected Collection<TruncateBehavior> TruncateBehaviors { get; private set; }

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
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors and configurations
            AcceptBehaviors = SetupBehaviors<AcceptBehavior>();
            TruncateBehaviors = SetupBehaviors<TruncateBehavior>();
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            foreach (var behavior in TruncateBehaviors)
                behavior.Unsetup();
            foreach (var behavior in AcceptBehaviors)
                behavior.Unsetup();
            TruncateBehaviors.Clear();
            TruncateBehaviors = null;
            AcceptBehaviors.Clear();
            AcceptBehaviors = null;

            base.Unsetup();
        }

        /// <summary>
        /// Execute the transient simulation
        /// </summary>
        protected override void Execute()
        {
            // First do temperature-dependent calculations and IC
            base.Execute();
            var exportargs = new ExportDataEventArgs(RealState, Method);

            var state = RealState;
            var baseConfig = BaseConfiguration;
            var timeConfig = TimeConfiguration;

            double delta = Math.Min(timeConfig.FinalTime / 50.0, timeConfig.Step) / 10.0;

            // Initialize before starting the simulation
            state.UseIC = timeConfig.UseIC;
            state.Domain = RealState.DomainType.Time;
            state.Gmin = baseConfig.Gmin;

            // Use node initial conditions if device initial conditions are not used
            if (!timeConfig.UseIC)
                OnLoad += LoadInitialConditions;

            // Calculate the operating point
            Op(baseConfig.DCMaxIterations);
            Statistics.TimePoints++;
            Method.DeltaOld.Clear(timeConfig.MaxStep);
            Method.Delta = delta;
            Method.SaveDelta = timeConfig.FinalTime / 50.0;

            // Stop calculating a DC solution
            state.UseIC = false;
            state.UseDC = false;
            foreach (var behavior in TransientBehaviors)
                behavior.GetDCState(this);
            StatePool.ClearDC();
            OnLoad -= LoadInitialConditions;

            // Start our statistics
            Statistics.TransientTime.Start();
            int startIters = Statistics.Iterations;
            var startselapsed = Statistics.SolveTime.Elapsed;

            try
            {
                while (true)
                {
                    // nextTime:

                    // Accept the current timepoint (CKTaccept())
                    foreach (var behavior in AcceptBehaviors)
                        behavior.Accept(this);
                    Method.SaveSolution(state.Solution);
                    // end of CKTaccept()

                    // Check if current breakpoint is outdated; if so, clear
                    Method.UpdateBreakpoints();
                    Statistics.Accepted++;

                    // Export the current timepoint
                    if (Method.Time >= timeConfig.InitTime)
                    {
                        Export(exportargs);
                    }

                    // Detect the end of the simulation
                    if (Method.Time >= timeConfig.FinalTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TransientIterations += Statistics.Iterations - startIters;
                        Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        OnLoad -= LoadInitialConditions;
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
                        if (Method.SavedTime == 0.0)
                            state.Init = RealState.InitializationStates.InitTransient;
                        bool converged = TranIterate(timeConfig.TranMaxIterations);
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

                            var data = new TimestepCutEventArgs(Circuit, Method.Delta / 8.0, TimestepCutEventArgs.TimestepCutReason.Convergence);
                            TimestepCut?.Invoke(this, data);
                        }
                        else
                        {
                            // Do not check the first time point
                            if (Method.SavedTime == 0.0 || Method.LteControl(this))
                            {
                                // goto nextTime;
                                break;
                            }
                            else
                            {
                                Statistics.Rejected++;
                                var data = new TimestepCutEventArgs(Circuit, Method.Delta, TimestepCutEventArgs.TimestepCutReason.Truncation);
                                TimestepCut?.Invoke(this, data);
                            }
                        }

                        if (Method.Delta <= timeConfig.DeltaMin)
                        {
                            if (Method.OldDelta > timeConfig.DeltaMin)
                                Method.Delta = timeConfig.DeltaMin;
                            else
                                throw new CircuitException("Timestep too small at t={0:g}: {1:g}".FormatString(Method.SavedTime, Method.Delta));
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
            var nodes = Circuit.Nodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.InitialConditions.ContainsKey(node.Name))
                {
                    double ic = nodes.InitialConditions[node.Name];
                    if (ZeroNoncurRow(state.Matrix, nodes, node.Index))
                    {
                        state.Rhs[node.Index] = 1.0e10 * ic;
                        node.Diagonal.Value = 1.0e10;
                    }
                    else
                    {
                        state.Rhs[node.Index] = ic;
                        node.Diagonal.Value = 1.0;
                    }
                }
            }
        }
    }
}
