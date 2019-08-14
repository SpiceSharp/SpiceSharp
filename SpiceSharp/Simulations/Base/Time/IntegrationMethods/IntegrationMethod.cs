using System;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A template for integration methods.
    /// </summary>
    public abstract class IntegrationMethod
    {
        /// <summary>
        /// Gets the maximum integration order for the integration method.
        /// </summary>
        public int MaxOrder { get; }

        /// <summary>
        /// Gets the current integration order.
        /// </summary>
        public int Order { get; protected set; }

        /// <summary>
        /// Gets the previously accepted integration states.
        /// </summary>
        protected History<IntegrationState> IntegrationStates { get; }

        /// <summary>
        /// Class for managing integration states.
        /// </summary>
        protected StateManager StateManager { get; } = new StateManager();

        /// <summary>
        /// Gets the time of the last accepted timepoint.
        /// </summary>
        public double BaseTime { get; private set; }

        /// <summary>
        /// Gets the time of the currently probed timepoint.
        /// </summary>
        public virtual double Time { get; private set; }

        /// <summary>
        /// The first order derivative of any variable that is
        /// dependent on the timestep.
        /// </summary>
        public double Slope { get; protected set; }

        /// <summary>
        /// Occurs when truncating the probed timestep.
        /// </summary>
        public event EventHandler<TruncateTimestepEventArgs> TruncateProbe;

        /// <summary>
        /// Occurs when evaluating the current timestep after solving.
        /// </summary>
        public event EventHandler<TruncateEvaluateEventArgs> TruncateEvaluate;

        /// <summary>
        /// Occurs when the solution could not converge for the probed timepoint.
        /// </summary>
        public event EventHandler<TruncateEvaluateEventArgs> TruncateNonConvergence;

        /// <summary>
        /// Occurs when a timepoint is accepted.
        /// </summary>
        public event EventHandler<EventArgs> AcceptSolution;

        /// <summary>
        /// Occurs when the simulation decides on the next timestep to be probed.
        /// </summary>
        public event EventHandler<ModifyTimestepEventArgs> ContinueTimestep;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationMethod"/> class.
        /// </summary>
        /// <param name="maxOrder">The maximum integration order.</param>
        protected IntegrationMethod(int maxOrder)
        {
            if (maxOrder < 1)
                throw new CircuitException("Invalid order {0}".FormatString(maxOrder));
            MaxOrder = maxOrder;

            // Allocate history of timesteps and solutions
            IntegrationStates = new ArrayHistory<IntegrationState>(maxOrder + 2);
        }

        /// <summary>
        /// Sets up for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Setup(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var solver = simulation.RealState?.Solver;
            if (solver == null)
                throw new CircuitException("Could not extract solver");
            IntegrationStates.Clear(i => new IntegrationState(1.0, 
                new DenseVector<double>(solver.Order), 
                StateManager.Build()));
        }

        /// <summary>
        /// Creates a state that keeps a history of values.
        /// </summary>
        /// <returns>
        /// A <see cref="StateHistory"/> object that is compatible with this integration method.
        /// </returns>
        public virtual StateHistory CreateHistory() => new StateHistoryDefault(IntegrationStates, StateManager);

        /// <summary>
        /// Creates a state for which a derivative with respect to time can be determined.
        /// </summary>
        /// <param name="track">if set to <c>false</c>, the state is considered purely informative.</param>
        /// <returns>
        /// A <see cref="StateDerivative" /> object that is compatible with this integration method.
        /// </returns>
        /// <remarks>
        /// Tracked derivatives are used in more advanced features implemented by the integration method.
        /// For example, derived states can be used for finding a good time step by approximating the 
        /// local truncation error (ie. the error made by taking discrete time steps). If you do not 
        /// want the derivative to participate in these features, set <paramref name="track" /> to false.
        /// </remarks>
        public abstract StateDerivative CreateDerivative(bool track);

        /// <summary>
        /// Creates a state that can be derived and is tracked by the integration method.
        /// </summary>
        /// <returns>
        /// A <see cref="StateDerivative" /> object that is compatible with this integration method.
        /// </returns>
        public virtual StateDerivative CreateDerivative() => CreateDerivative(true);

        /// <summary>
        /// Initializes the integration method.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public virtual void Initialize(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Initialize variables
            Slope = 0.0;
            Order = 1;

            // Copy the first state to all other states (assume DC situation)
            simulation.RealState.Solution.CopyTo(IntegrationStates[0].Solution);
            for (var i = 1; i < IntegrationStates.Length; i++)
            {
                IntegrationStates[i].Delta = IntegrationStates[0].Delta;
                // IntegrationStates[0].Solution.CopyTo(IntegrationStates[i].Solution);
                IntegrationStates[0].State.CopyTo(IntegrationStates[i].State);
            }
        }

        /// <summary>
        /// Starts probing a new timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="delta">The timestep to be probed.</param>
        public virtual void Probe(TimeSimulation simulation, double delta)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Allow an additional truncation if necessary
            var args = new TruncateTimestepEventArgs(simulation, delta);
            OnTruncateProbe(args);

            // Advance the probing time
            Time = BaseTime + args.Delta;
            IntegrationStates[0].Delta = args.Delta;
        }

        /// <summary>
        /// Updates the integration method in case the solution did not converge.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="newDelta">The next timestep to be probed.</param>
        public virtual void NonConvergence(TimeSimulation simulation, out double newDelta)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Call event
            var args = new TruncateEvaluateEventArgs(simulation, MaxOrder);
            OnTruncateNonConvergence(args);

            // Set the new timestep
            newDelta = args.Delta;

            // Copy the last accepted solution back into the time simulation
            // The simulator could have diverged to some crazy value, so we'll start again from the last known correct solution
            IntegrationStates[1].Solution.CopyTo(simulation.RealState.Solution);
        }

        /// <summary>
        /// Evaluates whether or not the current solution can be accepted.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="newDelta">The next requested timestep in case the solution is not accepted.</param>
        /// <returns>
        ///   <c>true</c> if the time point is accepted; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Evaluate(TimeSimulation simulation, out double newDelta)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Call event
            var args = new TruncateEvaluateEventArgs(simulation, MaxOrder)
            {
                Order = Order
            };
            OnTruncateEvaluate(args);

            // Update values
            Order = args.Order;
            newDelta = args.Delta;
            return args.Accepted;
        }

        /// <summary>
        /// Accepts the last evaluated time point.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public virtual void Accept(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Store the current solution
            simulation.RealState?.Solution.CopyTo(IntegrationStates[0].Solution);
            OnAcceptSolution(EventArgs.Empty);
        }

        /// <summary>
        /// Continues the simulation.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        /// <param name="delta">The initial probing timestep.</param>
        public virtual void Continue(TimeSimulation simulation, ref double delta)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Allow registered methods to modify the timestep
            var args = new ModifyTimestepEventArgs(simulation, delta);
            OnContinue(args);

            // Shift the solutions and overwrite index 0 with the current solution
            IntegrationStates.Cycle();
            BaseTime = Time;

            // Update the new timestep
            IntegrationStates[0].Delta = args.Delta;
        }

        /// <summary>
        /// Destroys the integration method.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Unsetup(TimeSimulation simulation)
        {
            StateManager.Unsetup();

            // Clear the timesteps and solutions
            IntegrationStates.Clear((IntegrationState)null);

            // Clear variables
            Order = 0;
            Slope = 0.0;
            BaseTime = 0.0;
            Time = 0.0;
        }

        /// <summary>
        /// Gets a timestep in history. The index 0 is the current timestep.
        /// </summary>
        /// <param name="index">Points to go back in time.</param>
        /// <returns>
        /// The timestep.
        /// </returns>
        public double GetTimestep(int index) => IntegrationStates[index].Delta;

        /// <summary>
        /// Gets a solution in history. The index 0 is the last accepted solution.
        /// </summary>
        /// <param name="index">Points to go back in time.</param>
        /// <returns>The solution vector.</returns>
        public Vector<double> GetSolution(int index)
        {
            return IntegrationStates[index].Solution;
        }

        #if DEBUG
        /// <summary>
        /// Gets a state in history. The index 0 is the current timestep.
        /// </summary>
        /// <remarks>
        /// It is generally impossible to find out which state belongs to which entity behavior. A behavior will
        /// ask for a certain type of state, that works on a specific index in the vector in a way that depends on
        /// the integration method. This means that the states are pretty useless in this context, and can only really
        /// be used for debugging purposes.
        /// </remarks>
        /// <param name="index">Points to go back in time.</param>
        /// <returns>The state vector.</returns>
        public Vector<double> GetStates(int index)
        {
            return IntegrationStates[index].State;
        }
        #endif

        /// <summary>
        /// Raises the <see cref="E:TruncateNonConvergence" /> event.
        /// </summary>
        /// <param name="args">The <see cref="TruncateEvaluateEventArgs"/> instance containing the event data.</param>
        protected void OnTruncateNonConvergence(TruncateEvaluateEventArgs args) =>
            TruncateNonConvergence?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:TruncateEvaluate" /> event.
        /// </summary>
        /// <param name="args">The <see cref="TruncateEvaluateEventArgs"/> instance containing the event data.</param>
        protected void OnTruncateEvaluate(TruncateEvaluateEventArgs args) => TruncateEvaluate?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:AcceptSolution" /> event.
        /// </summary>
        protected void OnAcceptSolution(EventArgs args) => AcceptSolution?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:TruncateProbe" /> event.
        /// </summary>
        /// <param name="args">The <see cref="TruncateTimestepEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTruncateProbe(TruncateTimestepEventArgs args) => TruncateProbe?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="E:Continue" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ModifyTimestepEventArgs"/> instance containing the event data.</param>
        protected virtual void OnContinue(ModifyTimestepEventArgs args) => ContinueTimestep?.Invoke(this, args);
    }
}
