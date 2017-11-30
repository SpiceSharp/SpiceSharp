using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A time-domain analysis (Transient simulation)
    /// </summary>
    public class Transient : Simulation<Transient>
    {
        /// <summary>
        /// Gets or sets the initial timepoint that should be exported
        /// </summary>
        [SpiceName("init"), SpiceName("start"), SpiceInfo("The starting timepoint")]
        public double InitTime { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the final simulation timepoint
        /// </summary>
        [SpiceName("final"), SpiceName("stop"), SpiceInfo("The final timepoint")]
        public double FinalTime { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the step
        /// </summary>
        [SpiceName("step"), SpiceInfo("The timestep")]
        public double Step { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the maximum timestep
        /// </summary>
        [SpiceName("maxstep"), SpiceInfo("The maximum allowed timestep")]
        public double MaxStep
        {
            get
            {
                if (maxstep == 0.0 && !double.IsNaN(maxstep))
                    return (FinalTime - InitTime) / 50.0;
                return maxstep;
            }
            set { maxstep = value; }
        }
        private double maxstep;

        /// <summary>
        /// Get the minimum timestep allowed
        /// </summary>
        [SpiceName("deltamin"), SpiceInfo("The minimum delta for breakpoints")]
        public double DeltaMin { get { return 1e-13 * MaxStep; } }

        /// <summary>
        /// An event handler for when the timestep has been cut
        /// </summary>
        /// <param name="sender">The simulation that sends the event</param>
        /// <param name="ckt">The circuit</param>
        /// <param name="newstep">The timestep that will be tried next</param>
        public delegate void TimestepCutEventHandler(object sender, TimestepCutData data);

        /// <summary>
        /// Event that is called when the timestep has been cut due to convergence problems
        /// </summary>
        public event TimestepCutEventHandler TimestepCut;

        /// <summary>
        /// Private variables
        /// </summary>
        private List<CircuitObjectBehaviorLoad> loadbehaviors;
        private List<CircuitObjectBehaviorAccept> acceptbehaviors;
        private List<CircuitObjectBehaviorTruncate> truncatebehaviors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="config">The configuration</param>
        public Transient(string name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="step">The timestep</param>
        /// <param name="final">The final timepoint</param>
        public Transient(string name, double step, double final) : base(name)
        {
            Step = step;
            FinalTime = final;
        }

        /// <summary>
        /// Initialize the transient analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Initialize(Circuit ckt)
        {
            base.Initialize(ckt);
            loadbehaviors = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorLoad>(ckt);
            acceptbehaviors = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorAccept>(ckt);
            truncatebehaviors = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorTruncate>(ckt);
        }


        /// <summary>
        /// Execute the transient simulation
        /// </summary>
        protected override void Execute()
        {
            var ckt = Circuit;
            var state = ckt.State;
            var rstate = state;
            var config = CurrentConfig ?? throw new CircuitException("No configuration");
            var method = ckt.Method ?? (config.Method ?? throw new CircuitException("No integration method"));

            double delta = Math.Min(FinalTime / 50.0, Step) / 10.0;

            // Setup breakpoints
            method.Breaks.Clear();
            method.Breaks.SetBreakpoint(0.0);
            method.Breaks.SetBreakpoint(FinalTime);
            method.Breaks.MinBreak = MaxStep * 5e-5;

            // Initialize before starting the simulation
            state.UseIC = config.UseIC;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = CircuitState.DomainTypes.Time;
            state.Gmin = config.Gmin;

            // Setup breakpoints
            method.Initialize(ckt);
            state.ReinitStates(method);

            // Call events for initializing the simulation
            Initialize(ckt);

            // Calculate the operating point
            ckt.Method = null;
            ckt.Op(loadbehaviors, config, config.DcMaxIterations);
            ckt.Statistics.TimePoints++;
            for (int i = 0; i < method.DeltaOld.Length; i++)
                method.DeltaOld[i] = MaxStep;
            method.Delta = delta;
            method.SaveDelta = FinalTime / 50.0;

            // Initialize the method
            ckt.Method = method;

            // Stop calculating a DC solution
            state.UseIC = false;
            state.UseDC = false;
            for (int i = 0; i < state.States[0].Length; i++)
            {
                state.States[1][i] = state.States[0][i];
            }

            // Start our statistics
            ckt.Statistics.TransientTime.Start();
            int startIters = ckt.Statistics.NumIter;
            var startselapsed = ckt.Statistics.SolveTime.Elapsed;

            try
            {
                nextTime:

                // Accept the current timepoint (CKTaccept())
                foreach (var behavior in acceptbehaviors)
                    behavior.Accept(ckt);
                method.SaveSolution(rstate.Solution);
                // end of CKTaccept()

                // Check if current breakpoint is outdated; if so, clear
                method.UpdateBreakpoints();
                ckt.Statistics.Accepted++;

                // Export the current timepoint
                if (method.Time >= InitTime)
                    Export(ckt);

                // Detect the end of the simulation
                if (method.Time >= FinalTime)
                {
                    // Keep our statistics
                    ckt.Statistics.TransientTime.Stop();
                    ckt.Statistics.TranIter += ckt.Statistics.NumIter - startIters;
                    ckt.Statistics.TransientSolveTime += ckt.Statistics.SolveTime.Elapsed - startselapsed;

                    // Finished!
                    Finalize(ckt);
                    return;
                }

                // Pause test - pausing not supported

                // resume:
                method.Delta = Math.Min(method.Delta, MaxStep);
                method.Resume();
                state.ShiftStates();

                // Calculate a new solution
                while (true)
                {
                    method.TryDelta();

                    // Compute coefficients and predict a solution and reset states to our previous solution
                    method.ComputeCoefficients(ckt);
                    method.Predict(ckt);

                    // Try to solve the new point
                    if (method.SavedTime == 0.0)
                        state.Init = CircuitState.InitFlags.InitTransient;
                    bool converged = ckt.Iterate(loadbehaviors, config, config.TranMaxIterations);
                    ckt.Statistics.TimePoints++;
                    if (method.SavedTime == 0.0)
                    {
                        for (int i = 0; i < state.States[1].Length; i++)
                        {
                            state.States[2][i] = state.States[1][i];
                            state.States[3][i] = state.States[1][i];
                        }
                    }

                    // Spice copies the states the first time, we're not
                    // I believe this is because Spice treats the first timepoint after the OP as special (MODEINITTRAN)
                    // We don't treat it special (we just assume it started from a circuit in rest)

                    if (!converged)
                    {
                        // Failed to converge, let's try again with a smaller timestep
                        method.Rollback();
                        ckt.Statistics.Rejected++;
                        method.Delta /= 8.0;
                        method.CutOrder();

                        var data = new TimestepCutData(ckt, method.Delta / 8.0, TimestepCutData.TimestepCutReason.Convergence);
                        TimestepCut?.Invoke(this, data);
                    }
                    else
                    {
                        // Do not check the first time point
                        if (method.SavedTime == 0.0)
                            goto nextTime;

                        if (method.LteControl(ckt))
                            goto nextTime;
                        else
                        {
                            ckt.Statistics.Rejected++;
                            var data = new TimestepCutData(ckt, method.Delta, TimestepCutData.TimestepCutReason.Truncation);
                            TimestepCut?.Invoke(this, data);
                        }
                    }

                    if (method.Delta <= DeltaMin)
                    {
                        if (method.OldDelta > DeltaMin)
                            method.Delta = DeltaMin;
                        else
                            throw new CircuitException($"Timestep too small at t={method.SavedTime.ToString("g")}: {method.Delta.ToString("g")}");
                    }
                }
            }
            catch (CircuitException)
            {
                // Keep our statistics
                ckt.Statistics.TransientTime.Stop();
                ckt.Statistics.TranIter += ckt.Statistics.NumIter - startIters;
                ckt.Statistics.TransientSolveTime += ckt.Statistics.SolveTime.Elapsed - startselapsed;
                throw;
            }
        }
    }

    /// <summary>
    /// This class contains all data when a timestep cut event is triggered
    /// </summary>
    public class TimestepCutData
    {
        /// <summary>
        /// Enumerations
        /// </summary>
        public enum TimestepCutReason
        {
            Convergence, // Cut due to convergence problems
            Truncation // Cut due to the local truncation error
        }

        /// <summary>
        /// Get the circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// The new timestep that will be tried
        /// </summary>
        public double NewDelta { get; }

        /// <summary>
        /// Gets the reason for cutting the timestep
        /// </summary>
        public TimestepCutReason Reason { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ckt"></param>
        /// <param name="newdelta"></param>
        public TimestepCutData(Circuit ckt, double newdelta, TimestepCutReason reason)
        {
            Circuit = ckt;
            NewDelta = newdelta;
            Reason = reason;
        }
    }
}
