using System;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation that executes a transient analysis
    /// </summary>
    public class Transient : Simulation
    {
        /// <summary>
        /// Default configuration for transient simulations
        /// </summary>
        public static Configuration Default { get; } = new Configuration();

        /// <summary>
        /// A class that executes a transient simulation
        /// </summary>
        public class Configuration : SimulationConfiguration
        {
            /// <summary>
            /// Gets or sets the integration method used for this simulation
            /// </summary>
            public IntegrationMethod Method { get; set; } = new Trapezoidal();

            /// <summary>
            /// Gets or sets the maximum number of iterations when solving the operating point
            /// </summary>
            public int DcMaxIterations { get; set; } = 100;

            /// <summary>
            /// Gets or sets the maximum number of iterations when solving a new timestep
            /// </summary>
            public int TranMaxIterations { get; set; } = 100;

            /// <summary>
            /// Gets or sets the flag for using initial conditions without operating point
            /// </summary>
            public bool UseIC { get; set; } = false;

            /// <summary>
            /// Constructor
            /// </summary>
            public Configuration() { }
        }
        protected Configuration MyConfig { get { return (Configuration)Config ?? Default; } }

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
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="config">The configuration</param>
        public Transient(string name, Configuration config = null)
            : base(name, config ?? new Configuration())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="step">The timestep</param>
        /// <param name="final">The final time</param>
        public Transient(string name, object step, object stop)
            : base(name, new Configuration())
        {
            Set("step", step);
            Set("stop", stop);
        }

        /// <summary>
        /// Execute the transient simulation
        /// The timesteps are always too small... I can't find what it is, must have checked almost 10 times now.
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Execute(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state.Real;
            var method = MyConfig.Method;

            // Initialize
            state.UseIC = MyConfig.UseIC;
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = CircuitState.DomainTypes.Time;

            // Setup breakpoints
            method.Breaks.SetBreakpoint(InitTime);
            method.Breaks.SetBreakpoint(FinalTime);
            if (method.Breaks.MinBreak == 0.0)
                method.Breaks.MinBreak = 5e-5 * MaxStep;

            // Calculate the operating point
            Initialize(ckt);
            this.Op(ckt, MyConfig.DcMaxIterations);
            ckt.Statistics.TimePoints++;

            // Initialize the method
            ckt.Method = method;
            method.Initialize();
            method.DeltaMin = DeltaMin;
            method.FillOldDeltas(MaxStep);

            // Stop calculating a DC solution
            state.UseIC = false;
            state.UseDC = false;
            for (int i = 1; i < state.States.Length; i++)
                state.States[0].CopyTo(state.States[i]);

            // Start our statistics
            ckt.Statistics.TransientTime.Start();
            int startIters = ckt.Statistics.NumIter;
            var startselapsed = ckt.Statistics.SolveTime.Elapsed;

            try
            {
                while (true)
                {
                    // Accept the current timepoint
                    foreach (var c in ckt.Components)
                        c.Accept(ckt);
                    method.SaveSolution(rstate.Solution);

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
                        break;
                    }

                    // Advance time
                    double delta = method.Time > 0.0 ? method.Delta : Math.Min(FinalTime / 50, Step) / 10.0;
                    method.Advance(Math.Min(delta, MaxStep));
                    state.ShiftStates();

                    // Calculate a new solution
                    while (true)
                    {
                        double olddelta = method.Delta;

                        // Check validity of the delta
                        if (double.IsNaN(olddelta))
                            throw new CircuitException("Invalid timestep");

                        // Compute coefficients and predict a solution and reset states to our previous solution
                        method.ComputeCoefficients(ckt);
                        method.Predict(ckt);
                        state.States[1].CopyTo(state.States[0]);

                        // Try to solve the new point
                        bool converged = this.Iterate(ckt, MyConfig.TranMaxIterations);
                        ckt.Statistics.TimePoints++;

                        // Spice copies the states the first time, we're not
                        // I believe this is because Spice treats the first timepoint after the OP as special (MODEINITTRAN)
                        // We don't treat it special (we just assume it started from a circuit in rest)

                        if (!converged)
                        {
                            // Failed to converge, let's try again with a smaller timestep
                            ckt.Statistics.Rejected++;
                            var data = new TimestepCutData(ckt, method.Delta / 8.0, TimestepCutData.TimestepCutReason.Convergence);
                            TimestepCut?.Invoke(this, data);
                            method.Retry(method.Delta / 8.0);
                        }
                        else
                        {
                            // Spice does not truncate the first timepoint (it deliberately makes it small)
                            // We just check the first timepoint just like any other, and assume the circuit
                            // has always been at that voltage.

                            // Calculate a new value based on the local truncation error
                            if (!method.NewDelta(ckt))
                            {
                                // Reject the timepoint if the calculated timestep shrinks too fast
                                ckt.Statistics.Rejected++;
                                var data = new TimestepCutData(ckt, method.Delta, TimestepCutData.TimestepCutReason.Truncation);
                                TimestepCut?.Invoke(this, data);
                            }
                            else
                                break;
                        }

                        // Stop simulation if timesteps are consistently too small
                        if (method.Delta <= DeltaMin)
                        {
                            if (olddelta <= DeltaMin)
                            {
                                throw new CircuitException($"Timestep too small: {method.Delta}");
                            }
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

            Finalize(ckt);
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
