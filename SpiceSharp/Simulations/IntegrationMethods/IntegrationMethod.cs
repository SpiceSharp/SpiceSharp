using System;
using System.Collections.ObjectModel;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Provides methods for differential equation integration in time-domain analysis. This is an abstract class.
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
        /// Gets the configuration for the integration method
        /// </summary>
        public IntegrationConfiguration Config { get; } = new IntegrationConfiguration();

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
        public int MaxOrder { get; }

        /// <summary>
        /// Gets the current order
        /// </summary>
        public int Order { get; protected set; } = 0;

        /// <summary>
        /// Gets the current time
        /// </summary>
        public double Time { get; protected set; } = 0.0;

        /// <summary>
        /// Gets or sets the current timestep
        /// </summary>
        public double Delta { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the saved timestep
        /// </summary>
        public double SaveDelta { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the old timestep
        /// </summary>
        public double OldDelta { get; set; } = 0.0;

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
        public double[][] Solutions { get; protected set; } = null;

        /// <summary>
        /// Get the prediction for the next timestep
        /// </summary>
        public double[] Prediction { get; protected set; } = null;

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
        double savetime = double.NaN;
        Collection<TransientBehavior> transientBehaviors;

        /// <summary>
        /// Event called when the timestep needs to be truncated
        /// </summary>
        public event EventHandler<TruncationEventArgs> Truncate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration</param>
        /// <param name="maxorder">Maximum integration order</param>
        public IntegrationMethod(IntegrationConfiguration config, int maxorder)
        {
            MaxOrder = maxorder;
            Config = config ?? new IntegrationConfiguration();
            DeltaOld = new double[maxorder + 2];
            Solutions = new double[maxorder + 1][]; // new Vector<double>[MaxOrder + 1];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxorder">Maximum integration order</param>
        public IntegrationMethod(int maxorder)
        {
            MaxOrder = maxorder;
            Config = new IntegrationConfiguration();
            DeltaOld = new double[maxorder + 2];
            Solutions = new double[maxorder + 1][]; // new Vector<double>[MaxOrder + 1];
        }

        /// <summary>
        /// Save a solution for future integrations
        /// </summary>
        /// <param name="solution">The solution</param>
        public void SaveSolution(double[] solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // Now, move the solution vectors around
            if (Solutions[0] == null)
            {
                // No solutions yet, so allocate vectors
                for (int i = 0; i < Solutions.Length; i++)
                    Solutions[i] = new double[solution.Length]; // new DenseVector(solution.Count);
                Prediction = new double[solution.Length];
                // solution.CopyTo(Solutions[0]);
                for (int i = 0; i < solution.Length; i++)
                    Solutions[0][i] = solution[i];
            }
            else
            {
                // Cycle through solutions
                var tmp = Solutions[Solutions.Length - 1];
                for (int i = Solutions.Length - 1; i > 0; i--)
                    Solutions[i] = Solutions[i - 1];
                Solutions[0] = tmp;
                for (int i = 0; i < solution.Length; i++)
                    Solutions[0][i] = solution[i];
            }
        }

        /// <summary>
        /// Initialize/reset the integration method
        /// </summary>
        /// <param name="transientBehaviors">Truncation behaviors</param>
        public virtual void Initialize(Collection<TransientBehavior> transientBehaviors)
        {
            // Initialize variables
            Time = 0.0;
            savetime = 0.0;
            Delta = 0.0;
            Order = 1;
            Prediction = null;
            DeltaOld = new double[MaxOrder + 1];
            Solutions = new double[MaxOrder + 1][]; // new Vector<double>[MaxOrder + 1];

            // Register default truncation methods
            this.transientBehaviors = transientBehaviors;
            if (Config.TruncationMethod.HasFlag(IntegrationConfiguration.TruncationMethods.PerDevice))
                Truncate += TruncateDevices;
            if (Config.TruncationMethod.HasFlag(IntegrationConfiguration.TruncationMethods.PerNode))
                Truncate += TruncateNodes;

            // Last point was START so the current point is the point after a breakpoint (start)
            Break = true;
        }

        /// <summary>
        /// Advance the time with the specified timestep for the first time
        /// The actual timestep may be smaller due to breakpoints
        /// </summary>
        public void Resume()
        {
            // Are we at a breakpoint, or indistinguishably close?
            if ((Time == Breaks.First) || (Breaks.First - Time) <= DeltaMin)
            {
                // First timepoint after a breakpoint: cut integration order
                Order = 1;

                // Limit the next timestep if there is a breakpoint
                double mt = Math.Min(SaveDelta, Breaks.Delta);
                Delta = Math.Min(Delta, 0.1 * mt);

                // Spice will divide the delta by 10 in the first step
                if (SavedTime == 0.0)
                    Delta /= 10.0;

                // But we don't want to go below delmin for no reason
                Delta = Math.Max(Delta, DeltaMin * 2.0);
            }
            else if (Time + Delta >= Breaks.First)
            {
                // Breakpoint reached
                SaveDelta = Delta;
                Delta = Breaks.First - Time;

                // We reached a breakpoint!
                Break = true;
            }

            // Update old delta's with the current delta
            for (int i = DeltaOld.Length - 2; i >= 0; i--)
                DeltaOld[i + 1] = DeltaOld[i];
            DeltaOld[0] = Delta;
        }

        /// <summary>
        /// Try advancing time
        /// </summary>
        public void TryDelta()
        {
            // Check for invalid timesteps
            if (double.IsNaN(Delta))
                throw new CircuitException("Invalid timestep");

            OldDelta = Delta;
            savetime = Time;
            Time += Delta;
            DeltaOld[0] = Delta;
        }

        /// <summary>
        /// Roll back the time to the last advanced time and reset the order to 1
        /// </summary>
        public void Rollback() => Time = savetime;

        /// <summary>
        /// Go back to order 1
        /// </summary>
        public void CutOrder() => Order = 1;

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
        /// <param name="simulation">Time-based simulation</param>
        /// <returns>True if the timestep isn't cut</returns>
        public bool LteControl(TimeSimulation simulation)
        {
            // Invoke truncation event
            TruncationEventArgs args = new TruncationEventArgs(simulation, Delta);
            Truncate?.Invoke(this, args);
            double newdelta = args.Delta;

            if (newdelta > 0.9 * Delta)
            {
                if (Order == 1)
                {
                    Order = 2;

                    // Invoke truncation event
                    args = new TruncationEventArgs(simulation, Delta);
                    Truncate?.Invoke(this, args);
                    newdelta = args.Delta;

                    if (newdelta <= 1.05 * Delta)
                        Order = 1;
                }
                Delta = newdelta;
                return true;
            }

            // Truncation too strict, we'll have to recalculate the timepoint
            Rollback();
            Delta = newdelta;
            return false;
        }

        /// <summary>
        /// Remove breakpoints in the past
        /// </summary>
        public void UpdateBreakpoints()
        {
            while (Time > Breaks.First)
                Breaks.ClearBreakpoint();

            Break = false;
        }

        /// <summary>
        /// Integrate a state variable at a specific index
        /// </summary>
        /// <param name="first">The current point with state variables</param>
        /// <param name="index">The index of the state to be used</param>
        /// <returns></returns>
        public abstract void Integrate(HistoryPoint first, int index);

        /// <summary>
        /// Do truncation for all nodes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <returns></returns>
        protected abstract void TruncateNodes(object sender, TruncationEventArgs args);

        /// <summary>
        /// Do truncation for all devices
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <returns></returns>
        protected void TruncateDevices(object sender, TruncationEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            double timetmp = double.PositiveInfinity;
            foreach (var behavior in transientBehaviors)
                behavior.Truncate(ref timetmp);
            args.Delta = timetmp;
        }

        /// <summary>
        /// Calculate a prediction based on the current timestep
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void Predict(TimeSimulation simulation);

        /// <summary>
        /// Compute the coefficients needed for integration
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void ComputeCoefficients(TimeSimulation simulation);

        /// <summary>
        /// Calculate the new timestep based on the LTE (local truncation error)
        /// </summary>
        /// <param name="first">First point</param>
        /// <param name="index">Index</param>
        /// <param name="timestep">Timestep</param>
        public abstract void LocalTruncateError(HistoryPoint first, int index, ref double timestep);
    }
}
