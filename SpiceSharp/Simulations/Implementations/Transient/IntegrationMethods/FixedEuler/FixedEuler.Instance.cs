using SpiceSharp.Algebra;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class FixedEuler
    {
        /// <summary>
        /// An <see cref="IIntegrationMethod"/> for <see cref="FixedEuler"/>.
        /// </summary>
        /// <seealso cref="IIntegrationMethod" />
        protected partial class Instance : IIntegrationMethod
        {
            private double _fixedStep;
            private readonly IStateful<IBiasingSimulationState> _simulation;
            private int _stateValues = 0;
            private readonly IHistory<IVector<double>> _states = new ArrayHistory<IVector<double>>(2);
            private List<IIntegrationState> _registeredStates = new List<IIntegrationState>();

            /// <summary>
            /// Gets the maximum order of the integration method.
            /// </summary>
            /// <value>
            /// The maximum order.
            /// </value>
            public int MaxOrder => 1;

            /// <summary>
            /// Gets the order.
            /// </summary>
            /// <value>
            /// The order.
            /// </value>
            public int Order { get; set; }

            /// <summary>
            /// Gets the base timepoint in seconds from which the current timepoint is being probed.
            /// </summary>
            /// <value>
            /// The base time.
            /// </value>
            public double BaseTime { get; private set; }

            /// <summary>
            /// Gets the currently probed timepoint in seconds.
            /// </summary>
            /// <value>
            /// The current time.
            /// </value>
            public double Time { get; private set; }

            /// <summary>
            /// Gets the derivative factor of any quantity that is being derived
            /// by the integration method.
            /// </summary>
            /// <value>
            /// The slope.
            /// </value>
            public double Slope { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <param name="simulation">The simulation.</param>
            public Instance(FixedEuler parameters, IStateful<IBiasingSimulationState> simulation)
            {
                _fixedStep = parameters.InitialStep;
                _simulation = simulation.ThrowIfNull(nameof(simulation));
                Slope = 1.0 / _fixedStep;
                Order = 1;
            }

            /// <summary>
            /// Registers an integration state with the integration method.
            /// </summary>
            /// <param name="state">The integration state.</param>
            public void RegisterState(IIntegrationState state)
            {
                _registeredStates.Add(state);
            }

            /// <summary>
            /// Creates a derivative.
            /// </summary>
            /// <param name="track">If set to <c>true</c>, the integration method will use this state to limit truncation errors.</param>
            /// <returns>
            /// The derivative.
            /// </returns>
            public IDerivative CreateDerivative(bool track = true)
            {
                var result = new DerivativeInstance(this, _stateValues + 1);
                _stateValues += 2;
                return result;
            }

            /// <summary>
            /// Gets a previous solution used by the integration method. An index of 0 indicates the last accepted solution.
            /// </summary>
            /// <param name="index">The number of points to go back.</param>
            /// <returns>
            /// The previous solution.
            /// </returns>
            public IVector<double> GetPreviousSolution(int index) => null;

            /// <summary>
            /// Gets a previous timestep. An index of 0 indicates the current timestep.
            /// </summary>
            /// <param name="index">The number of points to go back.</param>
            /// <returns>
            /// The previous timestep.
            /// </returns>
            public double GetPreviousTimestep(int index) => _fixedStep;

            /// <summary>
            /// Initializes the integration method using the allocated biasing state.
            /// </summary>
            public void Initialize()
            {
                Time = 0.0;
                BaseTime = 0.0;

                // Copy the states
                _states.Set(i => new DenseVector<double>(_stateValues));
            }

            /// <summary>
            /// Initializes the integration states.
            /// </summary>
            public void InitializeStates()
            {
                foreach (var state in _states)
                    _states.Value.CopyTo(state);
            }

            /// <summary>
            /// Prepares the integration method for calculating the next timepoint.
            /// The integration method may change the suggested timestep if needed.
            /// </summary>
            public void Prepare()
            {
                _states.Accept();
                BaseTime = Time;
            }

            /// <summary>
            /// Probes a new timepoint.
            /// </summary>
            public void Probe()
            {
                Time = BaseTime + _fixedStep;
            }

            /// <summary>
            /// Evaluates the solution at the probed timepoint. If the solution is invalid,
            /// the analysis should roll back and try a smaller timestep.
            /// </summary>
            /// <returns>
            ///   <c>true</c> if the solution is a valid solution; otherwise, <c>false</c>.
            /// </returns>
            public bool Evaluate()
            {
                return true;
            }

            /// <summary>
            /// Accepts the last probed timepoint.
            /// </summary>
            public void Accept()
            {
                foreach (var state in _registeredStates)
                    state.Accept();
            }

            /// <summary>
            /// Rejects the last probed timepoint. This method can be called if no
            /// solution could be found.
            /// </summary>
            public void Reject()
            {
                throw new TimestepTooSmallException(0.0, BaseTime);
            }
        }
    }
}
