using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Histories;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public abstract partial class SpiceMethod
    {
        /// <summary>
        /// An instance of a Spice-based integration method.
        /// </summary>
        /// <seealso cref="IBreakpointMethod"/>
        protected abstract class SpiceInstance : IBreakpointMethod
        {
            private double _saveDelta;

            /// <inheritdoc/>
            public double Delta { get; protected set; }

            /// <summary>
            /// Gets the simulation state that keeps track of the simulation.
            /// </summary>
            /// <value>
            /// The simulation.
            /// </value>
            protected IBiasingSimulationState State { get; }

            /// <summary>
            /// Gets the history of integration states.
            /// </summary>
            /// <value>
            /// The history of integration states.
            /// </value>
            protected IHistory<SpiceIntegrationState> States { get; }

            /// <summary>
            /// Gets the registered states.
            /// </summary>
            /// <value>
            /// The registered states.
            /// </value>
            protected List<IIntegrationState> RegisteredStates { get; } = [];

            /// <summary>
            /// Gets the truncatable states.
            /// </summary>
            /// <value>
            /// The truncatable states.
            /// </value>
            protected List<ITruncatable> TruncatableStates { get; } = [];

            /// <inheritdoc/>
            public Breakpoints Breakpoints { get; } = new Breakpoints();

            /// <inheritdoc/>
            public bool Break { get; private set; }

            /// <inheritdoc/>
            public int MaxOrder { get; }

            /// <inheritdoc/>
            public int Order { get; set; }

            /// <inheritdoc/>
            public double BaseTime { get; protected set; }

            /// <inheritdoc/>
            public double Time { get; protected set; }

            /// <inheritdoc/>
            public double Slope { get; protected set; }

            /// <summary>
            /// Gets the prediction.
            /// </summary>
            /// <value>
            /// The prediction.
            /// </value>
            protected IVector<double> Prediction { get; private set; }

            /// <summary>
            /// Gets the parameters.
            /// </summary>
            /// <value>
            /// The parameters.
            /// </value>
            protected SpiceMethod Parameters { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SpiceInstance"/> class.
            /// </summary>
            /// <param name="parameters">The method description.</param>
            /// <param name="state">The biasing simulation state.</param>
            /// <param name="maxOrder">The maximum order.</param>
            protected SpiceInstance(SpiceMethod parameters, IBiasingSimulationState state, int maxOrder)
            {
                Parameters = parameters.ThrowIfNull(nameof(parameters));
                State = state.ThrowIfNull(nameof(state));
                MaxOrder = maxOrder;
                States = new NodeHistory<SpiceIntegrationState>(maxOrder + 2);
            }

            /// <inheritdoc/>
            public abstract IDerivative CreateDerivative(bool track = true);

            /// <inheritdoc/>
            public abstract IIntegral CreateIntegral(bool track = true);

            /// <inheritdoc/>
            public IVector<double> GetPreviousSolution(int index)
                => States.GetPreviousValue(index).Solution;

            /// <inheritdoc/>
            public double GetPreviousTimestep(int index)
                => States.GetPreviousValue(index).Delta;

            /// <inheritdoc/>
            public void RegisterState(IIntegrationState state)
            {
                RegisteredStates.Add(state);
                if (state is ITruncatable truncatable)
                    TruncatableStates.Add(truncatable);
            }

            /// <inheritdoc/>
            public virtual void Initialize()
            {
                Time = 0.0;
                BaseTime = 0.0;
                Order = 1;
                Slope = 0.0;
                _saveDelta = double.PositiveInfinity;

                // Breakpoints
                Break = true;
                Breakpoints.Clear();
                Breakpoints.SetBreakpoint(Parameters.StartTime);
                Breakpoints.SetBreakpoint(Parameters.StopTime);

                // Create the prediction vector
                State.Solution.CopyTo(States.Value.Solution);
                Prediction = new DenseVector<double>(State.Solver.Size);

                // Calculate an initial timestep
                Delta = Math.Min(Parameters.StopTime / 50.0, Parameters.InitialStep) / 10.0;
            }

            /// <inheritdoc/>
            public virtual void Prepare()
            {
                double delta = Math.Min(Delta, Parameters.MaxStep);

                // Breakpoints
                if (Time.Equals(Breakpoints.First) || Breakpoints.First - Time <= Parameters.MinStep)
                {
                    // Cut integration order
                    Order = 1;

                    // Limit the next timestep
                    double mt = Math.Min(_saveDelta, Breakpoints.Delta);
                    delta = Math.Min(delta, 0.1 * mt);

                    // Spice will divide the first timestep by 10
                    if (BaseTime.Equals(0.0))
                        delta /= 10.0;

                    // Don't go below MinStep without reason
                    delta = Math.Max(delta, 2.0 * Parameters.MinStep);
                }
                else if (Time + delta >= Breakpoints.First)
                {
                    Break = true;
                    _saveDelta = delta;
                    delta = Breakpoints.First - Time;
                }

                // Start
                States.Accept();
                BaseTime = Time;
                States.Value.Delta = delta;
                Delta = delta;
            }

            /// <inheritdoc/>
            public virtual void Probe()
            {
                Time = BaseTime + Delta;
                States.Value.Delta = Delta;

                // Compute the integration coefficients
                ComputeCoefficients();
                Predict();
            }

            /// <summary>
            /// Computes the integration coefficients.
            /// </summary>
            protected abstract void ComputeCoefficients();

            /// <summary>
            /// Predicts a solution for truncation.
            /// </summary>
            protected abstract void Predict();

            /// <summary>
            /// Accepts a solution at the current timepoint.
            /// </summary>
            public virtual void Accept()
            {
                // Store the accepted solution
                State.Solution.CopyTo(States.Value.Solution);

                // When just starting out, we want to copy all the states to the previous states.
                if (BaseTime.Equals(0.0))
                {
                    foreach (var istate in States)
                    {
                        istate.Delta = Parameters.MaxStep;
                        States.Value.State.CopyTo(istate.State);
                    }
                }

                // Accept all the registered states
                foreach (var state in RegisteredStates)
                    state.Accept();

                // Clear the breakpoints
                while (Time > Breakpoints.First)
                    Breakpoints.ClearBreakpoint();
                Break = false;
            }

            /// <summary>
            /// Rejects the last probed timepoint as a valid solution. This method can be called if no solution could be found (eg. due to non-convergence).
            /// </summary>
            /// <exception cref="TimestepTooSmallException">Thrown when the timestep became too small.</exception>
            public virtual void Reject()
            {
                // Start once more from the last solution
                States.GetPreviousValue(1).Solution.CopyTo(State.Solution);

                // Is the previously tried timestep already at the minimum?
                if (States.Value.Delta <= Parameters.MinStep)
                    throw new TimestepTooSmallException(States.Value.Delta, BaseTime);

                // Limit the timestep and cut the order
                Delta = Math.Max(States.Value.Delta / 8.0, Parameters.MinStep);
                Order = 1;
            }

            /// <inheritdoc/>
            /// <exception cref="TimestepTooSmallException">Thrown when the timestep is too small.</exception>
            public virtual bool Evaluate(double maxTimestep)
            {
                double newDelta;
                maxTimestep.GreaterThan(nameof(maxTimestep), 0);

                // Ignore checks on the first timepoint
                if (BaseTime.Equals(0.0))
                {
                    if (maxTimestep < States.Value.Delta)
                        Delta = maxTimestep;
                    else
                        Delta = States.Value.Delta;
                    return true;
                }
                else
                {
                    newDelta = double.PositiveInfinity;

                    // Truncate the timestep
                    foreach (var truncatable in TruncatableStates)
                        newDelta = Math.Min(newDelta, truncatable.Truncate());
                    if (newDelta <= 0.0)
                        throw new TimestepTooSmallException(newDelta, BaseTime);

                    if (newDelta > 0.9 * States.Value.Delta)
                    {
                        if (Order < MaxOrder)
                        {
                            // Let's see if we can increase the timestep by using a bigger integration order
                            Order++;

                            // Truncate the timestep using the higher order
                            newDelta = double.PositiveInfinity;
                            foreach (var truncatable in TruncatableStates)
                                newDelta = Math.Min(newDelta, truncatable.Truncate());
                            if (newDelta <= 0.0)
                                throw new TimestepTooSmallException(newDelta, BaseTime);

                            // Is the higher (more expensive) order useful?
                            if (newDelta <= 1.05 * States.Value.Delta)
                                Order--;
                        }
                    }
                    else
                    {
                        Delta = Math.Min(newDelta, maxTimestep);
                        return false;
                    }
                }

                // Limit the expansion of the timestep
                newDelta = Math.Min(Parameters.MaximumExpansion * States.Value.Delta, newDelta);

                // Limit the maximum timestep
                if (newDelta > Parameters.MaxStep)
                    newDelta = Parameters.MaxStep;
                if (newDelta > maxTimestep)
                    newDelta = maxTimestep;

                // Check for timesteps that became too small
                if (newDelta <= Parameters.MinStep)
                {
                    // Was the previously tried timestep already at the minimum?
                    if (States.Value.Delta <= Parameters.MinStep)
                        throw new TimestepTooSmallException(newDelta, BaseTime);

                    // Else let's just try one more time with the minimum timestep
                    newDelta = Parameters.MinStep;
                    Order = 1;
                }

                Delta = newDelta;
                return true;
            }

            /// <inheritdoc/>
            public void Truncate(double maxTimestep)
            {
                maxTimestep.GreaterThan(nameof(maxTimestep), 0);
                if (maxTimestep < Delta)
                    Delta = maxTimestep;
            }
        }
    }
}
