using System;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SpiceSharp
{
    /// <summary>
    /// An abstract class that will describe integration methods
    /// </summary>
    public abstract class IntegrationMethod
    {
        /// <summary>
        /// This class represents the result of integration
        /// </summary>
        public class Result
        {
            public double Geq = 0.0;
            public double Ceq = 0.0;
        }

        /// <summary>
        /// This class represents an integration method configuration
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// Gets or sets the transient tolerance
            /// Used for timestep truncation
            /// </summary>
            public double TrTol { get; set; } = 7.0;

            /// <summary>
            /// Gets or sets the local truncation error relative tolerance
            /// Used for calculating a timestep based on the estimated error
            /// </summary>
            public double LteRelTol { get; set; } = 1e-3;

            /// <summary>
            /// Gets or sets the local truncation error absolute tolerance
            /// Used for calculating a timestep based on the estimated error
            /// </summary>
            public double LteAbsTol { get; set; } = 1e-6;
        }

        /// <summary>
        /// Gets the configuration for the integration method
        /// </summary>
        public Configuration Config { get; } = new Configuration();

        /// <summary>
        /// The breakpoints
        /// </summary>
        public Breakpoints Breaks { get; } = new Breakpoints();

        /// <summary>
        /// Gets whether a breakpoint was reached or not
        /// </summary>
        public bool Break { get; protected set; }

        /// <summary>
        /// Get the maximum order for the integration method
        /// </summary>
        public abstract int MaxOrder { get; }

        /// <summary>
        /// Gets the current order
        /// </summary>
        public int Order { get; protected set; } = 0;

        /// <summary>
        /// Gets the current time
        /// </summary>
        public double Time { get; protected set; } = 0.0;

        /// <summary>
        /// Gets or sets the current time step
        /// </summary>
        public double Delta { get; protected set; } = 0.0;

        /// <summary>
        /// Gets or sets the minimum delta timestep
        /// </summary>
        public double DeltaMin { get; set; } = 1e-12;

        /// <summary>
        /// Get the old time steps
        /// </summary>
        public double[] DeltaOld { get; protected set; } = null;

        /// <summary>
        /// Get the old solutions
        /// </summary>
        public Vector<double>[] Solutions { get; protected set; } = null;

        /// <summary>
        /// Get the prediction for the next timestep
        /// </summary>
        public Vector<double> Prediction { get; protected set; } = null;

        /// <summary>
        /// The first order derivative of any variable that is
        /// dependent on the timestep
        /// </summary>
        public double Slope { get; protected set; } = 0.0;

        /// <summary>
        /// Get the last time point that was accepted
        /// </summary>
        public double SavedTime { get { return savetime; } }

        /// <summary>
        /// Private variables
        /// </summary>
        private double savetime = double.NaN, savedelta = double.NaN;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration</param>
        public IntegrationMethod(Configuration config = null)
        {
            if (config == null)
                Config = new Configuration();
            else
                Config = config;

            DeltaOld = new double[MaxOrder + 2];
            Solutions = new Vector<double>[MaxOrder + 1];
        }

        /// <summary>
        /// Save a solution for future integrations
        /// </summary>
        /// <param name="solution">The solution</param>
        public void SaveSolution(Vector<double> solution)
        {
            if (Solutions[0] == null)
            {
                // No solutions yet, so allocate vectors
                for (int i = 0; i < Solutions.Length; i++)
                {
                    Solutions[i] = new DenseVector(solution.Count);
                    solution.CopyTo(Solutions[i]);
                }
            }
            else
            {
                // Cycle through solutions
                var tmp = Solutions[Solutions.Length - 1];
                for (int i = Solutions.Length - 1; i > 0; i--)
                    Solutions[i] = Solutions[i - 1];
                Solutions[0] = tmp;
                solution.CopyTo(Solutions[0]);
            }
        }

        /// <summary>
        /// Initialize/reset the integration method
        /// </summary>
        public virtual void Initialize()
        {
            // Initialize variables
            Time = 0.0;
            Breaks.Clear();
            Order = 1;
            Delta = double.NaN;
            Prediction = null;
            DeltaOld = new double[MaxOrder + 1];
            Solutions = new Vector<double>[MaxOrder + 1];
            savetime = double.NaN;
            savedelta = double.NaN;

            // Last point was START so the current point is the point after a breakpoint (start)
            Break = true;

            // Initialize the old timesteps - none
            for (int i = 0; i < DeltaOld.Length; i++)
                DeltaOld[i] = double.NaN;
        }

        /// <summary>
        /// Advance the time with the specified timestep for the first time
        /// The actual timestep may be smaller due to breakpoints
        /// </summary>
        /// <param name="delta">The timestep</param>
        public void Advance(double delta)
        {
            // Init
            Break = false;
            savetime = Time;

            // First time advancing? Save it!
            if (double.IsNaN(savedelta))
                savedelta = delta;

            // Are we at a breakpoint, or indistinguishably close?
            if ((Time == Breaks.First) || (Breaks.First - Time) <= DeltaMin)
            {
                // First timepoint after a breakpoint: cut integration order
                Order = 1;

                // Limit the next timestep if there is a breakpoint
                double mt = Math.Min(savedelta, Breaks.Delta);
                delta = Math.Min(delta, 0.1 * mt);

                // Spice will divide the delta by 10 in the first step
                if (Time == 0.0)
                    delta /= 10.0;

                // But we don't want to go below delmin for no reason
                delta = Math.Max(delta, DeltaMin * 2.0);

                Time += delta;
            }
            else if (Time + delta >= Breaks.First)
            {
                // Breakpoint reached
                savedelta = delta;
                delta = Breaks.First - Time;

                // We reached a breakpoint!
                Break = true;
                Time = Breaks.First;
            }
            else
            {
                Time += delta;
            }

            // Update with the current delta
            Delta = delta;
            for (int i = DeltaOld.Length - 2; i >= 0; i--)
                DeltaOld[i + 1] = DeltaOld[i];
            DeltaOld[0] = Delta;
        }

        /// <summary>
        /// Retry a new timestep after the current one failed
        /// Will cut the order and cut the timestep
        /// </summary>
        /// <param name="delta">The new timestep</param>
        public void Retry(double delta)
        {
            if (delta > Delta)
                throw new CircuitException("The timestep can only shrink when retrying a new timestep");
            if (delta < DeltaMin)
                delta = DeltaMin;

            // Update all the variables
            Delta = delta;
            DeltaOld[0] = delta;
            Time = savetime + delta;

            // Cut the integration order
            Order = 1;
        }

        /// <summary>
        /// Calculate a new step
        /// The result is stored in Delta
        /// Note: This method does not advance time!
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns>True if the timestep isn't cut</returns>
        public bool NewDelta(Circuit ckt)
        {
            // Find the truncated value
            double olddelta = Delta;
            double newdelta = Truncate(ckt);
            bool result = true;

            // We can go up an order if the timestep did not shrink too much
            if (newdelta > 0.9 * Delta)
            {
                if (Order == 1)
                {
                    // Increase the order and guess a new timepoint
                    Delta = olddelta;
                    Order = 2;
                    newdelta = Truncate(ckt);

                    // Not worth the computational effort
                    if (newdelta <= 1.05 * Delta)
                        Order = 1;
                }
                newdelta = Math.Max(newdelta, DeltaMin);
                result = true;
            }
            else
            {

                // Rewind the time
                newdelta = Math.Max(newdelta, DeltaMin);
                Time = savetime + newdelta;
                result = false;
            }

            // Return the result
            Delta = newdelta;
            DeltaOld[0] = newdelta;
            return result;
        }

        /// <summary>
        /// Remove breakpoints in the past
        /// </summary>
        public void UpdateBreakpoints()
        {
            while (Time > Breaks.First)
                Breaks.ClearBreakpoint();
        }

        /// <summary>
        /// Reset all old timesteps to a fixed value
        /// </summary>
        /// <param name="delta">The timestep</param>
        public void FillOldDeltas(double delta)
        {
            for (int i = 0; i < DeltaOld.Length; i++)
                DeltaOld[i] = delta;
        }

        /// <summary>
        /// Integrate a state variable
        /// Note that the integrated quantity will/should be stored at the next index!
        /// </summary>
        /// <param name="state">The state of the circuit</param>
        /// <param name="index">The index of the variable to be integrated</param>
        /// <param name="cap">The capacitance</param>
        /// <returns></returns>
        public abstract Result Integrate(CircuitState state, int index, double cap);

        /// <summary>
        /// Integrate a state variable
        /// Note that the integrated quantity will/should be stored at the next index!
        /// </summary>
        /// <param name="state">The state of the circuit</param>
        /// <param name="geq">The Geq parameter</param>
        /// <param name="ceq">The Ceq parameter</param>
        /// <param name="index">The index of the variable to be integrated</param>
        /// <param name="cap">The capacitance</param>
        public void Integrate(CircuitState state, out double geq, out double ceq, int index, double cap)
        {
            var result = Integrate(state, index, cap);
            geq = result.Geq;
            ceq = result.Ceq;
        }

        /// <summary>
        /// Truncate to relax timestep if possible
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public abstract double Truncate(Circuit ckt);

        /// <summary>
        /// Calculate a prediction based on the current timestep
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Predict(Circuit ckt);

        /// <summary>
        /// Compute the coefficients needed for integration
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void ComputeCoefficients(Circuit ckt);
    }
}
