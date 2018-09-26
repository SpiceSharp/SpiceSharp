using System;
using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods.Spice;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// The default integration method as implemented by Spice 3f5
    /// </summary>
    public abstract class SpiceIntegrationMethod : IntegrationMethod, IBreakpoints
    {
        /// <summary>
        /// The breakpoints
        /// </summary>
        public Breakpoints Breakpoints { get; } = new Breakpoints();

        /// <summary>
        /// True if we just hit a breakpoint earlier
        /// </summary>
        public bool Break { get; protected set; }

        /// <summary>
        /// Transient tolerance correction factor
        /// </summary>
        protected double TrTol { get; private set; } = 7.0;

        /// <summary>
        /// Allowed relative tolerance for the local truncation error
        /// </summary>
        protected double LteRelTol { get; private set; } = 1e-3;

        /// <summary>
        /// Allowed absolute tolerance for the local truncation error
        /// </summary>
        protected double LteAbsTol { get; private set; } = 1e-6;

        /// <summary>
        /// Allowed tolerance on charge
        /// </summary>
        protected double ChgTol { get; private set; } = 1e-14;

        /// <summary>
        /// Allowed absolute tolerance
        /// </summary>
        protected double AbsTol { get; private set; } = 1e-12;

        /// <summary>
        /// Allowed relative tolerance
        /// </summary>
        protected double RelTol { get; private set; } = 1e-3;

        /// <summary>
        /// Allowed maximum timestep
        /// </summary>
        protected double MaxStep { get; private set; } = 1e-6;

        /// <summary>
        /// Expansion factor
        /// </summary>
        protected double Expansion { get; private set; } = 2.0;

        /// <summary>
        /// Minimum timestep
        /// </summary>
        protected double MinStep { get; private set; }

        /// <summary>
        /// Gets the prediction vector
        /// </summary>
        protected Vector<double> Prediction { get; private set; }

        /// <summary>
        /// Keep track of all states that can be truncated
        /// </summary>
        protected List<ITruncatable> TruncatableStates { get; } = new List<ITruncatable>();

        /// <summary>
        /// Private variables
        /// </summary>
        private double _saveDelta;
        private double _oldDelta;

        /// <summary>
        /// Constructor
        /// </summary>
        protected SpiceIntegrationMethod(int maxOrder)
            : base(maxOrder)
        {
            TruncateEvaluate += TruncateStates;
        }

        /// <summary>
        /// Setup the integration method
        /// </summary>
        /// <param name="simulation">Time simulation</param>
        public override void Setup(TimeSimulation simulation)
        {
            base.Setup(simulation);

            // Base configuration
            var bp = simulation.ParameterSets.Get<BaseConfiguration>();
            AbsTol = bp.AbsoluteTolerance;
            RelTol = bp.RelativeTolerance;

            // Basic time configuration
            var tc = simulation.ParameterSets.Get<TimeConfiguration>();
            Breakpoints.SetBreakpoint(tc.InitTime);
            Breakpoints.SetBreakpoint(tc.FinalTime);
            MaxStep = tc.MaxStep;
            MinStep = tc.DeltaMin;
            // _saveDelta = tc.FinalTime / 50.0;
            _saveDelta = double.PositiveInfinity;

            // Detect spice configuration
            if (simulation.ParameterSets.TryGet(out SpiceConfiguration sc))
            {
                TrTol = sc.TrTol;
                LteRelTol = sc.LteRelTol;
                LteAbsTol = sc.LteAbsTol;
                ChgTol = sc.ChgTol;
                Expansion = sc.Expansion;
            }

            // Allocate a new vector for predictions
            Prediction = new DenseVector<double>(simulation.RealState.Solver.Order);
        }

        /// <summary>
        /// Initialize the integration method
        /// </summary>
        /// <param name="simulation">The simulation</param>
        public override void Initialize(TimeSimulation simulation)
        {
            base.Initialize(simulation);

            // The first point will be after t=0
            Break = true;

            // Assume a circuit in DC
            foreach (var state in IntegrationStates)
                state.Delta = MaxStep;
        }

        /// <summary>
        /// Accept the current time point as a final solution
        /// </summary>
        /// <param name="simulation">The simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Clear breakpoints
            while (Time > Breakpoints.First)
                Breakpoints.ClearBreakpoint();
            Break = false;

            base.Accept(simulation);
        }

        /// <summary>
        /// Continue
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="delta">Delta</param>
        public override void Continue(TimeSimulation simulation, ref double delta)
        {
            // Modify the timestep
            delta = Math.Min(delta, MaxStep);

            // Handle breakpoints
            if (Time.Equals(Breakpoints.First) || Breakpoints.First - Time <= MinStep)
            {
                // Cut integration order
                Order = 1;

                // Limit the next timestep
                var mt = Math.Min(_saveDelta, Breakpoints.Delta);
                delta = Math.Min(delta, 0.1 * mt);

                // Spice will divide the first timestep by 10
                if (BaseTime.Equals(0.0))
                    delta /= 10.0;

                // Don't go below MinStep without reason
                delta = Math.Max(delta, 2.0 * MinStep);
            }
            else if (Time + delta >= Breakpoints.First)
            {
                Break = true;
                _saveDelta = delta;
                delta = Breakpoints.First - Time;
            }

            base.Continue(simulation, ref delta);
        }

        /// <summary>
        /// Probe a new time point
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="delta">Timestep</param>
        public override void Probe(TimeSimulation simulation, double delta)
        {
            base.Probe(simulation, delta);

            ComputeCoefficients();
            Predict(simulation);

            // Save the current timestep
            _oldDelta = IntegrationStates[0].Delta;
        }

        /// <summary>
        /// Indicate that the solution did not converge
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="newDelta">The new timestep</param>
        public override void NonConvergence(TimeSimulation simulation, out double newDelta)
        {
            base.NonConvergence(simulation, out newDelta);

            // Limit the timestep and cut the order
            newDelta = Math.Min(newDelta, IntegrationStates[0].Delta / 8.0);
            Order = 1;
        }

        /// <summary>
        /// Evaluate the found solution
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="newDelta">The new timestep</param>
        /// <returns></returns>
        public override bool Evaluate(TimeSimulation simulation, out double newDelta)
        {
            var result = base.Evaluate(simulation, out newDelta);

            // Limit the expansion of the timestep
            newDelta = Math.Min(Expansion * IntegrationStates[0].Delta, newDelta);

            // Limit the maximum timestep
            if (newDelta > MaxStep)
                newDelta = MaxStep;

            // If the timestep is consistently made smaller than the minimum timestep, throw an exception
            if (newDelta <= MinStep)
            {
                // If we already tried
                if (_oldDelta <= MinStep)
                    throw new CircuitException("Timestep {0:e} is too small at time {1:e}".FormatString(newDelta, BaseTime));
                newDelta = MinStep;
            }

            return result;
        }

        /// <summary>
        /// Unsetup the integration method
        /// </summary>
        public override void Unsetup()
        {
            base.Unsetup();

            // Clear prediction
            Prediction = null;

            // Clear all truncatable states
            TruncatableStates.Clear();

            // Remove all breakpoints
            Breakpoints.Clear();
            Break = false;
        }

        /// <summary>
        /// Create a state that can be derived
        /// </summary>
        /// <remarks>
        /// Tracked derivatives are used in more advanced features by the integration method if they
        /// are implemented. For example, derived states can be used for finding a good time step
        /// by approximating the local truncation error (ie. the error made by taking discrete
        /// time steps). If you do not want the derivative to participate in these features, set
        /// <see cref="track"/> to false.
        /// </remarks>
        /// <param name="track">If false, this derivative is treated as purely informative</param>
        /// <returns></returns>
        public override StateDerivative CreateDerivative(bool track)
        {
            var ds = ProduceDerivative();
            if (track && ds is ITruncatable ts)
                TruncatableStates.Add(ts);
            return ds;
        }

        /// <summary>
        /// Compute the coefficients for the current timestep
        /// </summary>
        protected abstract void ComputeCoefficients();

        /// <summary>
        /// Predict a solution
        /// </summary>
        /// <param name="simulation">The simulation</param>
        protected abstract void Predict(TimeSimulation simulation);

        /// <summary>
        /// Factory for derivative states used by this integration method
        /// </summary>
        /// <returns></returns>
        protected abstract StateDerivative ProduceDerivative();

        /// <summary>
        /// Truncate the timestep using states
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected virtual void TruncateStates(object sender, TruncateEvaluateEventArgs args)
        {
            // Don't truncate the first step
            if (BaseTime.Equals(0.0))
                return;

            // Truncate!
            var newDelta = args.Delta;
            foreach (var state in TruncatableStates)
                newDelta = Math.Min(newDelta, state.Truncate());

            if (newDelta > 0.9 * IntegrationStates[0].Delta)
            {
                if (Order < MaxOrder)
                {
                    // Try increasing the order
                    Order++;
                    args.Order = Order;

                    // Try truncation again
                    newDelta = args.Delta;
                    foreach (var state in TruncatableStates)
                        newDelta = Math.Min(newDelta, state.Truncate());

                    // Increasing the order doesn't make a significant difference
                    if (newDelta <= 1.05 * IntegrationStates[0].Delta)
                    {
                        Order--;
                        args.Order = Order;
                    }
                }
            }
            else
            {
                args.Accepted = false;
            }

            args.Delta = newDelta;
        }

        /// <summary>
        /// Truncate the timestep using nodes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected abstract void TruncateNodes(object sender, TruncateEvaluateEventArgs args);
    }
}
