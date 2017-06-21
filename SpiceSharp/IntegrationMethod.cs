using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Private variables
        /// </summary>
        private double savetime = 0.0, savedelta = 0.0;

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
                for (int i = Solutions.Length - 2; i >= 0; i--)
                    Solutions[i + 1] = Solutions[i];
                Solutions[0] = tmp;
                solution.CopyTo(Solutions[0]);
            }
        }

        /// <summary>
        /// Reset the integration method
        /// </summary>
        public virtual void Initialize(double savedelta)
        {
            Time = 0.0;
            Breaks.Clear();
            Prediction = null;
            Order = 1;
            Delta = double.NaN;
            DeltaOld = new double[MaxOrder + 2];
            Solutions = new Vector<double>[MaxOrder + 1];
            Break = true;

            for (int i = 0; i < DeltaOld.Length; i++)
                DeltaOld[i] = double.NaN;
            this.savedelta = savedelta;
        }

        /// <summary>
        /// Advance the time with the specified timestep
        /// Breakpoints will cut the timestep.
        /// </summary>
        /// <param name="delta">The timestep</param>
        public void Advance(double delta)
        {
            Break = false;

            // Are we at a breakpoint, or indistinguishably close?
            if ((Time == Breaks.First) || (Breaks.First - Time) <= DeltaMin)
            {
                // First timepoint after a breakpoint: cut integration order
                Order = 1;

                // Limit the next timestep
                double mt = Math.Min(savedelta, Breaks.Delta);
                delta = Math.Min(delta, 0.1 * mt);

                // Spice will divide the delta by 10 in the first step, we don't

                // But we don't want to go below delmin for no reason
                delta = Math.Max(delta, DeltaMin * 2.0);
            }
            else if (Time + delta >= Breaks.First)
            {
                // Breakpoint reached
                savedelta = delta;
                delta = Breaks.First - Time;
                Break = true;
            }

            // Update with the current delta
            Delta = delta;
            for (int i = DeltaOld.Length - 2; i >= 0; i--)
                DeltaOld[i + 1] = DeltaOld[i];
            DeltaOld[0] = Delta;
            savetime = Time;

            // Advance time with the new delta
            Time += Delta;
        }

        /// <summary>
        /// Roll back and try advancing again with a smaller delta
        /// </summary>
        /// <param name="delta">The new timestep</param>
        public void Retry(double delta)
        {
            if (delta > Delta)
                throw new CircuitException("The timestep can only shrink when retrying a timestep");
            if (delta < DeltaMin)
                delta = DeltaMin;

            Delta = delta;
            DeltaOld[0] = delta;
            Time = savetime + delta;
            Order = 1;
        }

        /// <summary>
        /// Calculate a new timestep
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns>The calculated new timestep</returns>
        public bool NewDelta(Circuit ckt, out double newdelta)
        {
            // Find the truncated value
            newdelta = Truncate(ckt);

            // We can go up an order if the timestep did not shrink too much
            if (newdelta > 0.9 * Delta)
            {
                if (Order < MaxOrder)
                {
                    // Increase the order and guess a new timepoint
                    Order++;
                    newdelta = Truncate(ckt);

                    // Not worth the computational effort
                    if (newdelta <= 1.05 * Delta)
                        Order--;
                }
            }
            else
            {
                // Cut the order and try again, there might be something happening
                Order = 1;
                return false;
            }
            return true;
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
        /// Integrate a state variable
        /// Note that the integrated quantity will/should be stored at the next index!
        /// </summary>
        /// <param name="state">The state of the circuit</param>
        /// <param name="index">The index of the variable to be integrated</param>
        /// <param name="cap">The capacitance</param>
        /// <returns></returns>
        public abstract Result Integrate(CircuitState state, int index, double cap);

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
