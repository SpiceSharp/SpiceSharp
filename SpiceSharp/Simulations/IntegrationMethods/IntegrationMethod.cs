using System;
using System.Collections.ObjectModel;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Provides methods for differential equation integration in time-domain analysis. This is an abstract class.
    /// </summary>
    public abstract class IntegrationMethod
    {
        /// <summary>
        /// Gets the parameters for the integration method
        /// </summary>
        public ParameterSetDictionary Parameters { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets the base parameters for the integration method
        /// </summary>
        protected IntegrationParameters BaseParameters { get; private set; }

        /// <summary>
        /// The breakpoints
        /// </summary>
        public Breakpoints Breaks { get; } = new Breakpoints();

        /// <summary>
        /// Gets whether a breakpoint was reached or not
        /// </summary>
        public bool Break { get; protected set; }

        /// <summary>
        /// Gets the maximum order for the integration method
        /// </summary>
        public int MaxOrder { get; }

        /// <summary>
        /// Gets the current order
        /// </summary>
        public int Order { get; protected set; }

        /// <summary>
        /// Gets the current time
        /// </summary>
        public double Time { get; protected set; }

        /// <summary>
        /// Gets or sets the current timestep
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// Gets or sets the saved timestep
        /// </summary>
        public double SaveDelta { get; set; }

        /// <summary>
        /// Gets or sets the old timestep
        /// </summary>
        public double OldDelta { get; set; }

        /// <summary>
        /// Gets or sets the minimum delta timestep
        /// </summary>
        public double DeltaMin { get; set; } = 1e-12;

        /// <summary>
        /// Gets the old time steps
        /// </summary>
        public History<double> DeltaOld { get; }

        /// <summary>
        /// Gets the old solutions
        /// </summary>
        public History<Vector<double>> Solutions { get; }

        /// <summary>
        /// Gets the prediction for the next timestep
        /// </summary>
        public Vector<double> Prediction { get; protected set; }

        /// <summary>
        /// The first order derivative of any variable that is
        /// dependent on the timestep
        /// </summary>
        public double Slope { get; protected set; } = 0.0;

        /// <summary>
        /// Gets the last time point that was accepted
        /// </summary>
        public double SavedTime => _savetime;

        /// <summary>
        /// Private variables
        /// </summary>
        private double _savetime = double.NaN;
        private Collection<TransientBehavior> _transientBehaviors;

        /// <summary>
        /// Event called when the timestep needs to be truncated
        /// </summary>
        public event EventHandler<TruncationEventArgs> Truncate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <param name="maxOrder">Maximum integration order</param>
        protected IntegrationMethod(IntegrationParameters configuration, int maxOrder)
        {
            if (maxOrder < 1)
                throw new CircuitException("Invalid order {0}".FormatString(maxOrder));
            MaxOrder = maxOrder;

            // Allocate history of timesteps
            DeltaOld = new ArrayHistory<double>(maxOrder + 2);

            // Allocate history of solutions
            Solutions = new ArrayHistory<Vector<double>>(maxOrder + 1);

            // Create configuration if necessary
            Parameters.Add(configuration ?? new IntegrationParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxOrder">Maximum integration order</param>
        protected IntegrationMethod(int maxOrder)
        {
            if (maxOrder < 0)
                throw new CircuitException("Invalid order {0}".FormatString(maxOrder));
            MaxOrder = maxOrder;

            // Allocate history of timesteps
            DeltaOld = new ArrayHistory<double>(maxOrder + 2);

            // Allocate history of solutions
            Solutions = new ArrayHistory<Vector<double>>(maxOrder + 1);

            // Create configuration
            Parameters.Add(new IntegrationParameters());
        }

        /// <summary>
        /// Save a solution for future integrations
        /// </summary>
        /// <param name="solution">The solution</param>
        public void SaveSolution(Vector<double> solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // Now, move the solution vectors around
            if (Solutions[0] == null)
            {
                // No solutions yet, so allocate vectors
                Solutions.Clear(index => new DenseVector<double>(solution.Length));
                Prediction = new DenseVector<double>(solution.Length);
                solution.CopyTo(Solutions[0]);
            }
            else
            {
                // Cycle through solutions
                Solutions.Cycle();
                solution.CopyTo(Solutions[0]);
            }
        }

        /// <summary>
        /// Initialize/reset the integration method
        /// </summary>
        /// <param name="behaviors">Truncation behaviors</param>
        public virtual void Initialize(Collection<TransientBehavior> behaviors)
        {
            // Initialize variables
            Time = 0.0;
            _savetime = 0.0;
            Delta = 0.0;
            Order = 1;
            Prediction = null;
            DeltaOld.Clear(0.0);
            Solutions.Clear((Vector<double>)null);

            // Get parameters
            BaseParameters = Parameters.Get<IntegrationParameters>();

            // Register default truncation methods
            _transientBehaviors = behaviors;
            if (BaseParameters.TruncationMethod.HasFlag(IntegrationParameters.TruncationMethods.PerDevice))
                Truncate += TruncateDevices;
            if (BaseParameters.TruncationMethod.HasFlag(IntegrationParameters.TruncationMethods.PerNode))
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
            if (Time.Equals(Breaks.First) || Breaks.First - Time <= DeltaMin)
            {
                // First timepoint after a breakpoint: cut integration order
                Order = 1;

                // Limit the next timestep if there is a breakpoint
                double mt = Math.Min(SaveDelta, Breaks.Delta);
                Delta = Math.Min(Delta, 0.1 * mt);

                // Spice will divide the delta by 10 in the first step
                if (SavedTime.Equals(0.0))
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
            DeltaOld.Store(Delta);
        }

        /// <summary>
        /// Try advancing time
        /// </summary>
        public void TryDelta()
        {
            // Check for invalid timesteps
            if (double.IsNaN(Delta))
                throw new CircuitException("Invalid time step");

            OldDelta = Delta;
            _savetime = Time;
            Time += Delta;
            DeltaOld.Current = Delta;
        }

        /// <summary>
        /// Roll back the time to the last advanced time and reset the order to 1
        /// </summary>
        public void Rollback() => Time = _savetime;

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
                throw new CircuitException("The time step can only shrink when retrying a new timestep");
            if (delta < DeltaMin)
                delta = DeltaMin;

            // Update all the variables
            Delta = delta;
            DeltaOld.Current = delta;
            Time = _savetime + delta;

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
        /// <param name="history">The history</param>
        /// <param name="index">The index of the state to be used</param>
        /// <returns></returns>
        public abstract void Integrate(History<Vector<double>> history, int index);

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
            foreach (var behavior in _transientBehaviors)
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
        /// <param name="history">The history of states</param>
        /// <param name="index">Index</param>
        /// <param name="timestep">Timestep</param>
        public abstract void LocalTruncateError(History<Vector<double>> history, int index, ref double timestep);
    }
}
