using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    public partial class Gear
    {
        /// <summary>
        /// A state that can be derived by the Gear method 
        /// </summary>
        protected class GearStateDerivative : StateDerivative, ITruncatable
        {
            // Private variables
            private readonly int _index;
            private readonly Gear _method;
            private readonly History<IntegrationState> _states;

            /// <summary>
            /// Gets or sets the current value
            /// </summary>
            public override double Current
            {
                get => _states[0].State[_index];
                set => _states[0].State[_index] = value;
            }

            /// <summary>
            /// Gets a previous state value
            /// </summary>
            /// <param name="index">Number of points to go back</param>
            /// <returns></returns>
            public override double this[int index] => _states[index].State[_index];

            /// <summary>
            /// Gets the derivative of the state
            /// </summary>
            public override double Derivative => _states[0].State[_index + 1];

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="method">Parent integration method</param>
            public GearStateDerivative(Gear method)
            {
                _method = method;
                _index = method.StateManager.AllocateState(1);
                _states = method.IntegrationStates;
            }

            /// <summary>
            /// Calculate the jacobian
            /// </summary>
            /// <param name="derivative">Derivative</param>
            /// <returns></returns>
            public override double Jacobian(double derivative) => derivative * _method.Slope;

            /// <summary>
            /// Calculate the RHS-vector contribution for linear components
            /// </summary>
            /// <returns></returns>
            public override double RhsCurrent() =>
                _states[0].State[_index + 1] - _method.Slope * _states[0].State[_index];

            /// <summary>
            /// Calculate the derivative
            /// </summary>
            public override void Integrate()
            {
                var derivativeIndex = _index + 1;
                var ag = _method.Coefficients;

                var current = _states[0].State;
                current[derivativeIndex] = 0.0;
                for (var i = 0; i <= _method.Order; i++)
                    current[derivativeIndex] += ag[i] * _states[i].State[_index];
            }

            /// <summary>
            /// Truncate the time step
            /// </summary>
            /// <returns></returns>
            public double Truncate()
            {
                var derivativeIndex = _index + 1;
                var current = _states[0].State;
                var previous = _states[1].State;

                var diff = new double[_method.MaxOrder + 2];
                var deltmp = new double[_states.Length];

                // Calculate the tolerance
                var volttol =
                    _method.AbsTol + _method.RelTol * Math.Max(Math.Abs(current[derivativeIndex]), Math.Abs(previous[derivativeIndex]));
                var chargetol = Math.Max(Math.Abs(current[_index]), Math.Abs(previous[_index]));
                chargetol = _method.RelTol * Math.Max(chargetol, _method.ChgTol) / _states[0].Delta;
                var tol = Math.Max(volttol, chargetol);

                // Now compute divided differences
                var j = 0;
                foreach (var state in _states)
                {
                    diff[j] = state.State[_index];
                    deltmp[j] = state.Delta;
                    j++;
                }

                j = _method.Order;
                while (true)
                {
                    for (var i = 0; i <= j; i++)
                        diff[i] = (diff[i] - diff[i + 1]) / deltmp[i];
                    if (--j < 0)
                        break;
                    for (var i = 0; i <= j; i++)
                        deltmp[i] = deltmp[i + 1] + _states[i].Delta;
                }

                // Calculate the new timestep
                double factor = double.NaN;
                switch (_method.Order)
                {
                    case 1: factor = 0.5;
                        break;
                    case 2: factor = 0.2222222222;
                        break;
                    case 3: factor = 0.1363636364;
                        break;
                    case 4: factor = 0.096;
                        break;
                    case 5: factor = 0.07299270073;
                        break;
                    case 6: factor = 0.05830903790;
                        break;
                }

                var del = _method.TrTol * tol / Math.Max(_method.AbsTol, factor * Math.Abs(diff[0]));
                if (_method.Order == 2)
                    del = Math.Sqrt(del);
                else if (_method.Order > 2)
                    del = Math.Exp(Math.Log(del) / _method.Order);
                return del;
            }
        }
    }
}
