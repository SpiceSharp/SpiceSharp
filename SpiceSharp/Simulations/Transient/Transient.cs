using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A time-domain analysis (Transient simulation)
    /// </summary>
    public class Transient : TimeSimulation
    {
        /// <summary>
        /// Event handler when cutting a timestep
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="data">Timestep cut data</param>
        public delegate void TimestepCutEventHandler(object sender, TimestepCutData data);

        /// <summary>
        /// Event that is called when the timestep has been cut due to convergence problems
        /// </summary>
        public event TimestepCutEventHandler TimestepCut;

        /// <summary>
        /// Private variables
        /// </summary>
        private List<AcceptBehavior> acceptbehaviors;
        private List<TruncateBehavior> truncatebehaviors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        public Transient(Identifier name) : base(name)
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
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get behaviors
            acceptbehaviors = SetupBehaviors<AcceptBehavior>();
            truncatebehaviors = SetupBehaviors<TruncateBehavior>();

            // Setup the behaviors for usage with our matrix
            var matrix = Circuit.State.Matrix;
            foreach (var behavior in loadbehaviors)
                behavior.GetMatrixPointers(Circuit.Nodes, matrix);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            foreach (var behavior in truncatebehaviors)
                behavior.Unsetup();
            foreach (var behavior in acceptbehaviors)
                behavior.Unsetup();
            truncatebehaviors.Clear();
            truncatebehaviors = null;
            acceptbehaviors.Clear();
            acceptbehaviors = null;

            base.Unsetup();
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

            double delta = Math.Min(FinalTime / 50.0, Step) / 10.0;

            // Initialize before starting the simulation
            state.UseIC = config.UseIC;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = State.DomainTypes.Time;
            state.Gmin = config.Gmin;

            // Setup breakpoints
            Method.Initialize(ckt, truncatebehaviors);
            state.Initialize(ckt);
            state.ReinitStates(Method);

            // Calculate the operating point
            ckt.Method = null;
            Op(ckt, config.DcMaxIterations);
            ckt.Statistics.TimePoints++;
            for (int i = 0; i < Method.DeltaOld.Length; i++)
            {
                Method.DeltaOld[i] = MaxStep;
            }
            Method.Delta = delta;
            Method.SaveDelta = FinalTime / 50.0;

            // Initialize the Method
            ckt.Method = Method;

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
                while (true)
                {
                    // nextTime:

                    // Accept the current timepoint (CKTaccept())
                    foreach (var behavior in acceptbehaviors)
                        behavior.Accept(ckt);
                    Method.SaveSolution(rstate.Solution);
                    // end of CKTaccept()

                    // Check if current breakpoint is outdated; if so, clear
                    Method.UpdateBreakpoints();
                    ckt.Statistics.Accepted++;

                    // Export the current timepoint
                    if (Method.Time >= InitTime)
                    {
                        Export(ckt);
                    }

                    // Detect the end of the simulation
                    if (Method.Time >= FinalTime)
                    {
                        // Keep our statistics
                        ckt.Statistics.TransientTime.Stop();
                        ckt.Statistics.TranIter += ckt.Statistics.NumIter - startIters;
                        ckt.Statistics.TransientSolveTime += ckt.Statistics.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Pause test - pausing not supported

                    // resume:
                    Method.Delta = Math.Min(Method.Delta, MaxStep);
                    Method.Resume();
                    state.ShiftStates();

                    // Calculate a new solution
                    while (true)
                    {
                        Method.TryDelta();

                        // Compute coefficients and predict a solution and reset states to our previous solution
                        Method.ComputeCoefficients(ckt);
                        Method.Predict(ckt);

                        // Try to solve the new point
                        if (Method.SavedTime == 0.0)
                            state.Init = State.InitFlags.InitTransient;
                        bool converged = TranIterate(ckt, config.TranMaxIterations);
                        ckt.Statistics.TimePoints++;
                        if (Method.SavedTime == 0.0)
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
                            Method.Rollback();
                            ckt.Statistics.Rejected++;
                            Method.Delta /= 8.0;
                            Method.CutOrder();

                            var data = new TimestepCutData(ckt, Method.Delta / 8.0, TimestepCutData.TimestepCutReason.Convergence);
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
                                ckt.Statistics.Rejected++;
                                var data = new TimestepCutData(ckt, Method.Delta, TimestepCutData.TimestepCutReason.Truncation);
                                TimestepCut?.Invoke(this, data);
                            }
                        }

                        if (Method.Delta <= DeltaMin)
                        {
                            if (Method.OldDelta > DeltaMin)
                                Method.Delta = DeltaMin;
                            else
                                throw new CircuitException($"Timestep too small at t={Method.SavedTime.ToString("g")}: {Method.Delta.ToString("g")}");
                        }
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
