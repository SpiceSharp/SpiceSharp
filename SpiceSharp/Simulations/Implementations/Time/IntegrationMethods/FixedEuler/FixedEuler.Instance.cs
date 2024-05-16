using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Histories;
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
            private readonly FixedEuler _parameters;
            private int _stateValues = 0;
            private readonly IHistory<IVector<double>> _states = new ArrayHistory<IVector<double>>(2);
            private readonly List<IIntegrationState> _registeredStates = [];

            /// <inheritdoc/>
            public int MaxOrder => 1;

            /// <inheritdoc/>
            public int Order { get; set; }

            /// <inheritdoc/>
            public double BaseTime { get; private set; }

            /// <inheritdoc/>
            public double Time { get; private set; }

            /// <inheritdoc/>
            public double Slope { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            public Instance(FixedEuler parameters)
            {
                _parameters = parameters.ThrowIfNull(nameof(parameters));
                Slope = 1.0 / _parameters.Step;
                Order = 1;
            }

            /// <inheritdoc/>
            public void RegisterState(IIntegrationState state)
            {
                _registeredStates.Add(state);
            }

            /// <inheritdoc/>
            public IDerivative CreateDerivative(bool track = true)
            {
                var result = new DerivativeInstance(this, _stateValues + 1);
                _stateValues += 2;
                return result;
            }

            /// <inheritdoc/>
            public IIntegral CreateIntegral(bool track = true)
            {
                var result = new IntegralInstance(this, _stateValues + 1);
                _stateValues += 2;
                return result;
            }

            /// <inheritdoc/>
            public IVector<double> GetPreviousSolution(int index) => null;

            /// <inheritdoc/>
            public double GetPreviousTimestep(int index) => _parameters.Step;

            /// <inheritdoc/>
            public void Initialize()
            {
                Time = 0.0;
                BaseTime = 0.0;

                // Copy the states
                _states.Set(i => new DenseVector<double>(_stateValues));
            }

            /// <inheritdoc/>
            public void Prepare()
            {
                _states.Accept();
                BaseTime = Time;
            }

            /// <inheritdoc/>
            public void Probe()
            {
                Time = BaseTime + _parameters.Step;
                Slope = 1.0 / _parameters.Step;
            }

            /// <inheritdoc/>
            /// <remarks>
            /// This method ignores any timesteps!
            /// </remarks>
            public bool Evaluate(double maxTimestep)
            {
                return true;
            }

            /// <inheritdoc/>
            public void Accept()
            {
                if (BaseTime.Equals(0.0))
                {
                    foreach (var state in _states)
                        _states.Value.CopyTo(state);
                }
                foreach (var state in _registeredStates)
                    state.Accept();
            }

            /// <inheritdoc/>
            public void Reject()
            {
                throw new TimestepTooSmallException(0.0, BaseTime);
            }

            /// <inheritdoc/>
            /// <remarks>
            /// This method ignores any timesteps!
            /// </remarks>
            public void Truncate(double maxTimestep)
            {
            }
        }
    }
}
