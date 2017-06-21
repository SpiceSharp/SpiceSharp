using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation that executes a transient analysis
    /// </summary>
    public class Transient : Simulation
    {
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
            /// Gets or sets the initial timepoint that should be exported
            /// </summary>
            public double InitTime { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the final simulation timepoint
            /// </summary>
            public double FinalTime { get; set; } = double.NaN;

            /// <summary>
            /// Gets or sets the maximum number of iterations when solving the operating point
            /// </summary>
            public int DcMaxIterations { get; set; } = 100;

            /// <summary>
            /// Gets or sets the maximum number of iterations when solving a new timestep
            /// </summary>
            public int TranMaxIterations { get; set; } = 100;

            /// <summary>
            /// Gets or sets the step
            /// </summary>
            public double Step { get; set; } = double.NaN;

            /// <summary>
            /// Gets or sets the maximum timestep
            /// </summary>
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
            /// Gets or sets the flag for using initial conditions without operating point
            /// </summary>
            public bool UseIC { get; set; } = false;

            /// <summary>
            /// Get the minimum timestep allowed
            /// </summary>
            public double DeltaMin { get { return 1e-9 * MaxStep; } }

            /// <summary>
            /// Constructor
            /// </summary>
            public Configuration() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="step">The time step</param>
            /// <param name="final">The final time</param>
            public Configuration(double step, double final)
            {
                Step = step;
                FinalTime = final;
            }
        }

        /// <summary>
        /// Get the current configuration
        /// </summary>
        protected Configuration MyConfig { get { return (Configuration)Config;  } }

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
        public Transient(string name, double step, double final)
            : base(name, new Configuration(step, final))
        {
        }

        /// <summary>
        /// Execute the transient simulation
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

            // Initialize
            method.Initialize(MyConfig.FinalTime / 50.0);
            method.Breaks.SetBreakpoint(MyConfig.InitTime);
            method.Breaks.SetBreakpoint(MyConfig.FinalTime);
            if (method.Breaks.MinBreak == 0.0)
                method.Breaks.MinBreak = 5e-5 * MyConfig.MaxStep;
            ckt.Method = MyConfig.Method;
            for (int i = 0; i < method.DeltaOld.Length; i++)
                method.DeltaOld[i] = MyConfig.MaxStep;

            // Calculate the operating point
            this.Op(ckt, MyConfig.DcMaxIterations);
            ckt.Statistics.TimePoints++;

            // Stop calculating a DC solution
            state.UseIC = false;
            state.UseDC = false;
            for (int i = 1; i < state.States.Length; i++)
                state.States[0].CopyTo(state.States[i]);

            // Start our statistics
            ckt.Statistics.TransientTime.Start();
            int startIters = ckt.Statistics.NumIter;
            var startselapsed = ckt.Statistics.SolveTime.Elapsed;
            
            double delta = Math.Min(MyConfig.FinalTime / 50.0, MyConfig.Step) / 10.0;
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
                    if (method.Time >= MyConfig.InitTime)
                        Export(ckt);

                    // Detect the end of the simulation
                    if (method.Time >= MyConfig.FinalTime)
                    {
                        // Keep our statistics
                        ckt.Statistics.TransientTime.Stop();
                        ckt.Statistics.TranIter += ckt.Statistics.NumIter - startIters;
                        ckt.Statistics.TransientSolveTime += ckt.Statistics.SolveTime.Elapsed - startselapsed;
                        break;
                    }

                    // Advance time
                    delta = Math.Min(delta, MyConfig.MaxStep);
                    method.Advance(delta);
                    state.ShiftStates();

                    // Calculate a new solution
                    while (true)
                    {
                        double olddelta = method.Delta;

                        // Compute coefficients and predict a solution
                        method.ComputeCoefficients(ckt);
                        method.Predict(ckt);

                        // Try to solve the new point
                        bool converged = this.Iterate(ckt, MyConfig.TranMaxIterations);
                        ckt.Statistics.TimePoints++;

                        // Spice copies the states the first time, we're not
                        // I believe this is because Spice treats the first timepoint after the OP as special (MODEINITTRAN)
                        // We don't treat it special (we just assume it started from rest)

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
                            // Spice does not check the first timepoint (it deliberately makes it small)
                            // We just check the first timepoint just like any other, and assume the circuit
                            // has always been at that voltage.

                            // Calculate a new value based on the local truncation error
                            if (!method.NewDelta(ckt, out delta))
                            {
                                // Reject the timepoint if the calculated timestep shrinks too fast
                                ckt.Statistics.Rejected++;
                                var data = new TimestepCutData(ckt, delta, TimestepCutData.TimestepCutReason.Truncation);
                                TimestepCut?.Invoke(this, data);
                                method.Retry(delta);
                            }
                            else
                            {
                                // Everything went fine, we have a solution!
                                break;
                            }
                        }

                        // Stop simulation if timesteps are consistently too small
                        if (method.Delta <= MyConfig.DeltaMin)
                        {
                            if (olddelta <= MyConfig.DeltaMin)
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
            Convergence, // Cut due to convergence
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
