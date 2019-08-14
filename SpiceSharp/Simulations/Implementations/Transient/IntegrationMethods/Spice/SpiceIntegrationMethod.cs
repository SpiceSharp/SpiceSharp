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
        /// Gets the breakpoint system.
        /// </summary>
        public Breakpoints Breakpoints { get; } = new Breakpoints();

        /// <summary>
        /// Gets a value indicating whether this point is the first after a breakpoint.
        /// </summary>
        public bool Break { get; protected set; }

        /// <summary>
        /// Gets the transient tolerance correction factor.
        /// </summary>
        protected double TrTol { get; private set; } = 7.0;

        /// <summary>
        /// Gets or sets the local truncation error relative tolerance.
        /// </summary>
        protected double LteRelTol { get; private set; } = 1e-3;

        /// <summary>
        /// Gets or sets the local truncation truncation error absolute tolerance.
        /// </summary>
        protected double LteAbsTol { get; private set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance for charges.
        /// </summary>
        protected double ChgTol { get; private set; } = 1e-14;

        /// <summary>
        /// Gets the allowed absolute tolerance.
        /// </summary>
        protected double AbsTol { get; private set; } = 1e-12;

        /// <summary>
        /// Gets the allowed relative tolerance.
        /// </summary>
        protected double RelTol { get; private set; } = 1e-3;

        /// <summary>
        /// Gets the maximum timestep.
        /// </summary>
        protected double MaxStep { get; private set; } = 1e-6;

        /// <summary>
        /// Gets the timestep expansion factor.
        /// </summary>
        protected double Expansion { get; private set; } = 2.0;

        /// <summary>
        /// Gets the minimum timestep.
        /// </summary>
        protected double MinStep { get; private set; }

        /// <summary>
        /// Gets the prediction vector.
        /// </summary>
        protected Vector<double> Prediction { get; private set; }

        /// <summary>
        /// Gets a list with all truncatable states.
        /// </summary>
        protected List<ITruncatable> TruncatableStates { get; } = new List<ITruncatable>();

        /// <summary>
        /// Private variables
        /// </summary>
        private double _saveDelta;
        private double _oldDelta;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceIntegrationMethod"/> class.
        /// </summary>
        /// <param name="maxOrder">The maximum integration order.</param>
        protected SpiceIntegrationMethod(int maxOrder)
            : base(maxOrder)
        {
            TruncateEvaluate += TruncateStates;
        }

        /// <summary>
        /// Sets up for the specified simulation.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Setup(TimeSimulation simulation)
        {
            base.Setup(simulation);

            // Base configuration
            var bp = simulation.Configurations.Get<BaseConfiguration>().ThrowIfNull("base configuration");
            AbsTol = bp.AbsoluteTolerance;
            RelTol = bp.RelativeTolerance;

            // Basic time configuration
            var tc = simulation.Configurations.Get<TimeConfiguration>().ThrowIfNull("time configuration");
            Breakpoints.SetBreakpoint(tc.InitTime);
            Breakpoints.SetBreakpoint(tc.FinalTime);
            MaxStep = tc.MaxStep;
            MinStep = tc.DeltaMin;
            // _saveDelta = tc.FinalTime / 50.0;
            _saveDelta = double.PositiveInfinity;

            // Detect spice configuration
            if (simulation.Configurations.TryGet(out SpiceConfiguration sc))
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
        /// Initializes the integration method.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
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
        /// Accepts the last evaluated time point.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Clear breakpoints
            while (Time > Breakpoints.First)
                Breakpoints.ClearBreakpoint();
            Break = false;

            base.Accept(simulation);
        }

        /// <summary>
        /// Continues the simulation.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        /// <param name="delta">The initial probing timestep.</param>
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
        /// Starts probing a new timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="delta">The timestep to be probed.</param>
        public override void Probe(TimeSimulation simulation, double delta)
        {
            base.Probe(simulation, delta);

            ComputeCoefficients();
            Predict(simulation);

            // Save the current timestep
            _oldDelta = IntegrationStates[0].Delta;
        }

        /// <summary>
        /// Updates the integration method in case the solution did not converge.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="newDelta">The next timestep to be probed.</param>
        public override void NonConvergence(TimeSimulation simulation, out double newDelta)
        {
            base.NonConvergence(simulation, out newDelta);

            // Limit the timestep and cut the order
            newDelta = Math.Min(newDelta, IntegrationStates[0].Delta / 8.0);
            Order = 1;

            // If the timestep is consistently made smaller than the minimum timestep, throw an exception
            if (newDelta <= MinStep)
            {
                // If we already tried
                if (_oldDelta <= MinStep)
                    throw new CircuitException("Timestep {0:e} is too small at time {1:e}".FormatString(newDelta, BaseTime));
                newDelta = MinStep;
            }
        }

        /// <summary>
        /// Evaluates whether or not the current solution can be accepted.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="newDelta">The next requested timestep in case the solution is not accepted.</param>
        /// <returns>
        /// <c>true</c> if the time point is accepted; otherwise, <c>false</c>.
        /// </returns>
        public override bool Evaluate(TimeSimulation simulation, out double newDelta)
        {
            // Spice 3f5 ignores the first timestep
            if (BaseTime.Equals(0.0))
            {
                newDelta = IntegrationStates[0].Delta;
                return true;
            }

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
        /// Destroys the integration method.
        /// </summary>
        public override void Unsetup(TimeSimulation simulation)
        {
            base.Unsetup(simulation);

            // Clear prediction
            Prediction = null;

            // Clear all truncatable states
            TruncatableStates.Clear();

            // Remove all breakpoints
            Breakpoints.Clear();
            Break = false;
        }

        /// <summary>
        /// Creates a state for which a derivative with respect to time can be determined.
        /// </summary>
        /// <param name="track">if set to <c>false</c>, the state is considered purely informative.</param>
        /// <returns>
        /// A <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> object that is compatible with this integration method.
        /// </returns>
        /// <remarks>
        /// Tracked derivatives are used in more advanced features implemented by the integration method.
        /// For example, derived states can be used for finding a good time step by approximating the
        /// local truncation error (ie. the error made by taking discrete time steps). If you do not
        /// want the derivative to participate in these features, set <paramref name="track" /> to false.
        /// </remarks>
        public override StateDerivative CreateDerivative(bool track)
        {
            var ds = ProduceDerivative();
            if (track && ds is ITruncatable ts)
                TruncatableStates.Add(ts);
            return ds;
        }

        /// <summary>
        /// Computes the integration coefficients.
        /// </summary>
        protected abstract void ComputeCoefficients();

        /// <summary>
        /// Predicts a solution
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        protected abstract void Predict(TimeSimulation simulation);

        /// <summary>
        /// Produces a derivative.
        /// </summary>
        /// <returns>
        /// A <see cref="StateDerivative" /> that can be used with this integration method.
        /// </returns>
        protected abstract StateDerivative ProduceDerivative();

        /// <summary>
        /// Truncates the timestep based on the states.
        /// </summary>
        /// <param name="sender">The sender (integration method).</param>
        /// <param name="args">The <see cref="TruncateEvaluateEventArgs"/> instance containing the event data.</param>
        protected virtual void TruncateStates(object sender, TruncateEvaluateEventArgs args)
        {
            args.ThrowIfNull(nameof(args));

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
        /// Truncates the timestep using nodes.
        /// </summary>
        /// <param name="sender">The sender (integration method).</param>
        /// <param name="args">The <see cref="TruncateEvaluateEventArgs"/> instance containing the event data.</param>
        protected abstract void TruncateNodes(object sender, TruncateEvaluateEventArgs args);
    }
}
