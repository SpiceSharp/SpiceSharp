using SpiceSharp.Algebra;
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
            /// <summary>
            /// Gets the timestep.
            /// </summary>
            /// <value>
            /// The timestep.
            /// </value>
            public double Delta { get; protected set; }

            /// <summary>
            /// Gets the simulation that the integration method will work with.
            /// </summary>
            /// <value>
            /// The simulation.
            /// </value>
            protected IStateful<IBiasingSimulationState> Simulation { get; }

            /// <summary>
            /// Gets the history of integration states.
            /// </summary>
            /// <value>
            /// The history of integration states.
            /// </value>
            protected IHistory<IntegrationState> States { get; }

            /// <summary>
            /// Gets the registered states.
            /// </summary>
            /// <value>
            /// The registered states.
            /// </value>
            protected List<IIntegrationState> RegisteredStates { get; } = new List<IIntegrationState>();

            /// <summary>
            /// Gets the truncatable states.
            /// </summary>
            /// <value>
            /// The truncatable states.
            /// </value>
            protected List<ITruncatable> TruncatableStates { get; } = new List<ITruncatable>();

            /// <summary>
            /// Private variables.
            /// </summary>
            private readonly double _maxStep, _minStep, _expansion, _tstart, _tstop, _step;
            private double _saveDelta, _oldDelta;

            /// <summary>
            /// Gets the absolute tolerance.
            /// </summary>
            /// <value>
            /// The absolute tolerance.
            /// </value>
            protected double AbsTol { get; }

            /// <summary>
            /// Gets the charge tolerance.
            /// </summary>
            /// <value>
            /// The charge tolerance.
            /// </value>
            protected double ChgTol { get; }

            /// <summary>
            /// Gets the relative tolerance.
            /// </summary>
            /// <value>
            /// The relative tolerance.
            /// </value>
            protected double RelTol { get; }

            /// <summary>
            /// Gets the transient tolerance factor.
            /// </summary>
            /// <value>
            /// The transient tolerance factor.
            /// </value>
            protected double TrTol { get; }

            /// <summary>
            /// Gets the local truncation error relative tolerance.
            /// </summary>
            /// <value>
            /// The relative tolerance.
            /// </value>
            protected double LteRelTol { get; }

            /// <summary>
            /// Gets the local truncation error absolute tolerance.
            /// </summary>
            /// <value>
            /// The absolute tolerance.
            /// </value>
            protected double LteAbsTol { get; }

            /// <summary>
            /// Gets the breakpoint system.
            /// </summary>
            public Breakpoints Breakpoints { get; } = new Breakpoints();

            /// <summary>
            /// Gets or sets whether a breakpoint has been reached.
            /// </summary>
            /// <value>
            ///   <c>true</c> if a breakpoint was reached; otherwise, <c>false</c>.
            /// </value>
            public bool Break { get; private set; }

            /// <summary>
            /// Gets the maximum order of the integration method.
            /// </summary>
            /// <value>
            /// The maximum order.
            /// </value>
            public int MaxOrder { get; }

            /// <summary>
            /// Gets or sets the current order of the integration method.
            /// </summary>
            /// <value>
            /// The current order.
            /// </value>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the base time.
            /// </summary>
            /// <value>
            /// The base time.
            /// </value>
            public double BaseTime { get; protected set; }

            /// <summary>
            /// Gets or sets the time.
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public double Time { get; protected set; }

            /// <summary>
            /// Gets or sets the slope.
            /// </summary>
            /// <value>
            /// The slope.
            /// </value>
            public double Slope { get; protected set; }

            /// <summary>
            /// Gets the prediction.
            /// </summary>
            /// <value>
            /// The prediction.
            /// </value>
            protected IVector<double> Prediction { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SpiceInstance"/> class.
            /// </summary>
            /// <param name="parameters">The method description.</param>
            /// <param name="simulation">The simulation that provides the biasing state.</param>
            /// <param name="maxOrder">The maximum order.</param>
            protected SpiceInstance(SpiceMethod parameters, IStateful<IBiasingSimulationState> simulation, int maxOrder)
            {
                Simulation = simulation.ThrowIfNull(nameof(simulation));
                MaxOrder = maxOrder;
                States = new NodeHistory<IntegrationState>(maxOrder + 2);

                // Store the necessary parameters
                _step = parameters.InitialStep;
                _minStep = parameters.MinStep;
                _maxStep = parameters.MaxStep;
                _tstart = parameters.StartTime;
                _tstop = parameters.StopTime;
                _expansion = parameters.Expansion;
                _saveDelta = double.PositiveInfinity;
                AbsTol = parameters.AbsTol;
                ChgTol = parameters.ChgTol;
                RelTol = parameters.RelTol;
                TrTol = parameters.TrTol;
                LteRelTol = parameters.LteRelTol;
                LteAbsTol = parameters.LteAbsTol;
            }

            /// <summary>
            /// Creates a derivative.
            /// </summary>
            /// <param name="track">If set to <c>true</c>, the integration method will use this state to limit truncation errors.</param>
            /// <returns>
            /// The derivative.
            /// </returns>
            public abstract IDerivative CreateDerivative(bool track);

            /// <summary>
            /// Gets a previous solution used by the integration method. An index of 0 indicates the last accepted solution.
            /// </summary>
            /// <param name="index">The number of points.</param>
            /// <returns>
            /// The previous solution.
            /// </returns>
            public IVector<double> GetPreviousSolution(int index)
                => States.GetPreviousValue(index).Solution;

            /// <summary>
            /// Gets a previous timestep. An index of 0 indicates the current timestep.
            /// </summary>
            /// <param name="index">The number of points to go back.</param>
            /// <returns>
            /// The previous timestep.
            /// </returns>
            public double GetPreviousTimestep(int index)
                => States.GetPreviousValue(index).Delta;

            /// <summary>
            /// Registers an integration state with the integration method.
            /// </summary>
            /// <param name="state">The integration state.</param>
            public void RegisterState(IIntegrationState state)
            {
                RegisteredStates.Add(state);
                if (state is ITruncatable truncatable)
                    TruncatableStates.Add(truncatable);
            }

            /// <summary>
            /// Initializes the integration method using the allocated biasing state.
            /// </summary>
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
                Breakpoints.SetBreakpoint(_tstart);
                Breakpoints.SetBreakpoint(_tstop);

                // Create the prediction vector
                Simulation.State.Solution.CopyTo(States.Value.Solution);
                Prediction = new DenseVector<double>(Simulation.State.Solver.Size);

                // Calculate an initial timestep
                Delta = Math.Min(_tstop / 50.0, _step) / 10.0;
            }

            /// <summary>
            /// Initializes the integration states.
            /// </summary>
            public virtual void InitializeStates()
            {
                foreach (var istate in States)
                {
                    istate.Delta = _maxStep;
                    States.Value.State.CopyTo(istate.State);
                }
            }

            /// <summary>
            /// Prepares the integration method for probing new values.
            /// </summary>
            public virtual void Prepare()
            {
                var delta = Math.Min(Delta, _maxStep);

                // Breakpoints
                if (Time.Equals(Breakpoints.First) || Breakpoints.First - Time <= _minStep)
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
                    delta = Math.Max(delta, 2.0 * _minStep);
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

            /// <summary>
            /// Probes a new timepoint.
            /// </summary>
            public virtual void Probe()
            {
                Time = BaseTime + Delta;
                States.Value.Delta = Delta;

                // Compute the integration coefficients
                ComputeCoefficients();
                Predict();

                // Save the current timestep
                _oldDelta = States.Value.Delta;
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
            /// Accepts the last probed timepoint.
            /// </summary>
            public virtual void Accept()
            {
                // Store the accepted solution
                Simulation.State.Solution.CopyTo(States.Value.Solution);

                // Accept all the registered states
                foreach (var state in RegisteredStates)
                    state.Accept();

                // Clear the breakpoints
                while (Time > Breakpoints.First)
                    Breakpoints.ClearBreakpoint();
                Break = false;
            }

            /// <summary>
            /// Rejects the last probed timepoint. This method can be called if no
            /// solution could be found.
            /// </summary>
            /// <exception cref="TimestepTooSmallException">Thrown when the timestep became too small.</exception>
            public virtual void Reject()
            {
                // TODO: We probably don't really need to keep the old delta.
                // Check if the last probed delta is smaller or equal to the min step.

                // Start once more from the last solution
                States.GetPreviousValue(1).Solution.CopyTo(Simulation.State.Solution);

                // Limit the timestep and cut the order
                Delta = States.Value.Delta / 8.0;
                Order = 1;

                // Check if we can't decrease the timestep further
                if (Delta <= _minStep)
                {
                    if (_oldDelta <= _minStep)
                        throw new TimestepTooSmallException(Delta, BaseTime);
                    Delta = _minStep;
                }
            }

            /// <summary>
            /// Evaluates the solution at the probed timepoint. If the solution is invalid,
            /// the analysis should roll back and try a smaller timestep.
            /// </summary>
            /// <returns>
            ///   <c>true</c> if the solution is a valid solution; otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="TimestepTooSmallException">Thrown when the timestep is too small.</exception>
            public virtual bool Evaluate()
            {
                double newDelta;

                // Ignore checks on the first timepoint
                if (BaseTime.Equals(0.0))
                {
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
                        Delta = newDelta;
                        return false;
                    }
                }

                // Limit the expansion of the timestep
                newDelta = Math.Min(_expansion * States.Value.Delta, newDelta);

                // Limit the maximum timestep
                if (newDelta > _maxStep)
                    newDelta = _maxStep;

                // Check for timesteps that are too small
                if (newDelta <= _minStep)
                {
                    // We already tried?
                    if (_oldDelta <= _minStep)
                        throw new TimestepTooSmallException(newDelta, BaseTime);

                    // One more time
                    newDelta = _minStep;
                    Order = 1;
                }

                Delta = newDelta;
                return true;
            }
        }
    }
}
