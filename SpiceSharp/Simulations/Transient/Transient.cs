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
        public event EventHandler<TimestepCutEventArgs> ConvergenceFailed;

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
                _acceptBehaviors[i].Unsetup(this);
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
                // var newDelta = Math.Min(timeConfig.FinalTime / 50.0, timeConfig.Step) / 10.0;
                var newDelta = 0.0001;
                while (true)
                {
                    // Accept the last evaluated time point
                    Method.Accept();
                    Statistics.Accepted++;

                    // Export the current timepoint
                    if (Method.Time >= timeConfig.InitTime)
                        OnExport(exportargs);

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

                    // Find a valid time point
                    while (true)
                    {
                        // Probe the next time point
                        Method.Probe(this, newDelta);

                        // Try to solve the new point
                        if (Method.BaseTime.Equals(0.0))
                            state.Init = RealState.InitializationStates.InitTransient;
                        var converged = TimeIterate(timeConfig.TranMaxIterations);
                        Statistics.TimePoints++;

                        // Did we fail to converge to a solution?
                        if (!converged)
                        {
                            var args = new TimestepCutEventArgs(0.0, TimestepCutEventArgs.TimestepCutReason.Convergence);
                            OnConvergenceFailed(args);
                        }

                        // If our integration method doesn't approve of our solution, retry probing a new timestep again
                        if (Method.Evaluate(this, out newDelta))
                            break;
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
        /// Call the event for when convergence fails
        /// </summary>
        /// <param name="args">Argument</param>
        protected void OnConvergenceFailed(TimestepCutEventArgs args)
        {
            ConvergenceFailed?.Invoke(this, args);
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
