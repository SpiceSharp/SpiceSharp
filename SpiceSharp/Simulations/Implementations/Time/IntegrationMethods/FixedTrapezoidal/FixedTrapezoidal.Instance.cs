using SpiceSharp.Algebra;
using SpiceSharp.Simulations.Histories;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class FixedTrapezoidal
    {
        /// <summary>
        /// An <see cref="IIntegrationMethod"/> for <see cref="FixedTrapezoidal"/>.
        /// </summary>
        /// <seealso cref="IIntegrationMethod"/>
        protected partial class Instance : IIntegrationMethod
        {
            private readonly FixedTrapezoidal _parameters;
            private int _stateValues = 0;
            private readonly IHistory<IVector<double>> _states = new ArrayHistory<IVector<double>>(3);
            private readonly List<IIntegrationState> _registeredStates = [];
            private readonly double _xmu;
            private double _ag0, _ag1;

            /// <inheritdoc />
            public int MaxOrder => 2;

            /// <inheritdoc />
            public int Order { get; set; }

            /// <inheritdoc />
            public double BaseTime { get; private set; }

            /// <inheritdoc />
            public double Time { get; private set; }

            /// <inheritdoc />
            public double Slope => _ag0;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            public Instance(FixedTrapezoidal parameters)
            {
                _parameters = parameters.ThrowIfNull(nameof(parameters));
                _xmu = parameters.Xmu;
                Order = 1;
            }

            /// <inheritdoc />
            public void RegisterState(IIntegrationState state)
            {
                _registeredStates.Add(state);
            }

            /// <inheritdoc />
            public IDerivative CreateDerivative(bool track = true)
            {
                var result = new DerivativeInstance(this, _stateValues + 1);
                _stateValues += 2;
                return result;
            }

            /// <inheritdoc />
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


            /// <inheritdoc />
            public void Initialize()
            {
                Time = 0.0;
                BaseTime = 0.0;

                // Copy the states
                _states.Set(i => new DenseVector<double>(_stateValues));
            }

            /// <inheritdoc />
            public void Prepare()
            {
                _states.Accept();
                BaseTime = Time;
            }

            /// <inheritdoc />
            public void Probe()
            {
                Time = BaseTime + _parameters.Step;

                switch (Order)
                {
                    case 1:
                        _ag0 = 1.0 / _parameters.Step;
                        _ag1 = -1.0 / _parameters.Step;
                        break;

                    case 2:
                        _ag0 = 1.0 / _parameters.Step / (1.0 - _xmu);
                        _ag1 = _xmu / (1.0 - _xmu);
                        break;
                }
            }

            /// <inheritdoc />
            /// <remarks>
            /// This method ignores any variable timesteps!
            /// </remarks>
            public bool Evaluate(double maxTimestep)
            {
                // We have two orders to play with, so increase if possible
                if (Order < MaxOrder)
                    Order++;
                return true;
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public void Reject()
            {
                throw new TimestepTooSmallException(0.0, BaseTime);
            }

            /// <inheritdoc />
            /// <remarks>
            /// This method ignores any variable timesteps!
            /// </remarks>
            public void Truncate(double maxTimestep)
            {
            }
        }
    }
}
