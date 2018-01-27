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
        public Transient(Identifier name) : base(name)
        {
            Parameters.Add(new TimeConfiguration());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="step">Step</param>
        /// <param name="final">Final time</param>
        public Transient(Identifier name, double step, double final) : base(name)
        {
            Parameters.Add(new TimeConfiguration(step, final));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="step">Step</param>
        /// <param name="final">Final time</param>
        /// <param name="maxstep">Maximum timestep</param>
        public Transient(Identifier name, double step, double final, double maxstep) : base(name)
        {
            Parameters.Add(new TimeConfiguration(step, final, maxstep));
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
            var exportargs = new ExportDataEventArgs(State, Method);

            var circuit = Circuit;
            var state = State;
            var baseconfig = BaseConfiguration;
            var timeconfig = TimeConfiguration;

            double delta = Math.Min(timeconfig.FinalTime / 50.0, timeconfig.Step) / 10.0;

            // Initialize before starting the simulation
            state.UseIC = timeconfig.UseIC;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = State.DomainTypes.Time;
            state.Gmin = baseconfig.Gmin;

            // Setup breakpoints
            Method.Initialize(TransientBehaviors);
            state.Initialize(circuit);

            // Calculate the operating point
            Op(baseconfig.DcMaxIterations);
            Statistics.TimePoints++;
            for (int i = 0; i < Method.DeltaOld.Length; i++)
            {
                Method.DeltaOld[i] = timeconfig.MaxStep;
            }
            Method.Delta = delta;
            Method.SaveDelta = timeconfig.FinalTime / 50.0;

            // Stop calculating a DC solution
            state.UseIC = false;
            state.UseDC = false;
            foreach (var behavior in TransientBehaviors)
                behavior.GetDCstate(this);
            States.ClearDC();

            // Start our statistics
            Statistics.TransientTime.Start();
            int startIters = Statistics.NumIter;
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
                    if (Method.Time >= timeconfig.InitTime)
                    {
                        Export(exportargs);
                    }

                    // Detect the end of the simulation
                    if (Method.Time >= timeconfig.FinalTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TranIter += Statistics.NumIter - startIters;
                        Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Pause test - pausing not supported

                    // resume:
                    Method.Delta = Math.Min(Method.Delta, timeconfig.MaxStep);
                    Method.Resume();
                    States.ShiftStates();

                    // Calculate a new solution
                    while (true)
                    {
                        Method.TryDelta();

                        // Compute coefficients and predict a solution and reset states to our previous solution
                        Method.ComputeCoefficients(this);
                        Method.Predict(this);

                        // Try to solve the new point
                        if (Method.SavedTime == 0.0)
                            state.Init = State.InitFlags.InitTransient;
                        bool converged = TranIterate(timeconfig.TranMaxIterations);
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

                            var data = new TimestepCutEventArgs(circuit, Method.Delta / 8.0, TimestepCutEventArgs.TimestepCutReason.Convergence);
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
                                var data = new TimestepCutEventArgs(circuit, Method.Delta, TimestepCutEventArgs.TimestepCutReason.Truncation);
                                TimestepCut?.Invoke(this, data);
                            }
                        }

                        if (Method.Delta <= timeconfig.DeltaMin)
                        {
                            if (Method.OldDelta > timeconfig.DeltaMin)
                                Method.Delta = timeconfig.DeltaMin;
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
                Statistics.TranIter += Statistics.NumIter - startIters;
                Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;
                throw new CircuitException("{0}: transient terminated".FormatString(Name), ex);
            }
        }
    }
}
