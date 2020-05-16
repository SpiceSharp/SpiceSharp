using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class Gear
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="IDerivative"/> for <see cref="Gear"/>.
            /// </summary>
            /// <seealso cref="IDerivative" />
            /// <seealso cref="ITruncatable" />
            protected class DerivativeInstance : IDerivative, ITruncatable
            {
                private readonly int _index;
                private readonly Instance _method;
                private readonly IHistory<SpiceIntegrationState> _states;

                /// <summary>
                /// Gets the current derivative.
                /// </summary>
                /// <value>
                /// The derivative.
                /// </value>
                public double Derivative => _states.Value.State[_index + 1];

                /// <summary>
                /// Gets or sets the current value.
                /// </summary>
                /// <value>
                /// The value.
                /// </value>
                public double Value
                {
                    get => _states.Value.State[_index];
                    set => _states.Value.State[_index] = value;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="DerivativeInstance"/> class.
                /// </summary>
                /// <param name="method">The method.</param>
                /// <param name="index">The index.</param>
                public DerivativeInstance(Instance method, int index)
                {
                    _method = method.ThrowIfNull(nameof(method));
                    _states = _method.States;
                    _index = index;
                }

                /// <summary>
                /// Gets the jacobian value and Rhs-vector value.
                /// </summary>
                /// <param name="coefficient">The coefficient of the quantity that is derived.</param>
                /// <param name="currentValue">The current value of the derived state.</param>
                /// <returns>
                /// The information for filling in the Y-matrix and Rhs-vector.
                /// </returns>
                public JacobianInfo GetContributions(double coefficient, double currentValue)
                {
                    double g = _method.Slope * coefficient;
                    return new JacobianInfo(
                        g,
                        Derivative - g * currentValue);
                }

                /// <summary>
                /// Gets the Y-matrix value and Rhs-vector contributions for the derived quantity.
                /// The relationship is assumed to be linear.
                /// </summary>
                /// <param name="coefficient">The coefficient of the quantity that is derived.</param>
                /// <returns>
                /// The information for filling in the Y-matrix and Rhs-vector
                /// </returns>
                public JacobianInfo GetContributions(double coefficient)
                {
                    var h = _method.Slope;
                    var s = _states.Value.State;
                    double g = h * coefficient;
                    return new JacobianInfo(
                        g,
                        s[_index + 1] - h * s[_index]);
                }

                /// <summary>
                /// Gets a previous value of the state. An index of 0 indicates the current value.
                /// </summary>
                /// <param name="index">The number of points to go back in time.</param>
                /// <returns>
                /// The previous value.
                /// </returns>
                public double GetPreviousValue(int index)
                    => _states.GetPreviousValue(index).State[_index];

                /// <summary>
                /// Gets a previous derivative of the state. An index of 0 indicates the current value.
                /// </summary>
                /// <param name="index">The number of points to go back in time.</param>
                /// <returns>
                /// The previous derivative.
                /// </returns>
                public double GetPreviousDerivative(int index)
                    => _states.GetPreviousValue(index).State[_index + 1];

                /// <summary>
                /// Integrates the state (calculates the derivative).
                /// </summary>
                public void Integrate()
                {
                    var derivativeIndex = _index + 1;
                    var ag = _method.Coefficients;

                    var current = _states.Value.State;
                    current[derivativeIndex] = 0.0;
                    for (var i = 0; i <= _method.Order; i++)
                        current[derivativeIndex] += ag[i + 1] * _states.GetPreviousValue(i).State[_index];
                }

                /// <summary>
                /// Truncates the current timestep.
                /// </summary>
                /// <returns>
                /// The maximum timestep allowed by this state.
                /// </returns>
                public double Truncate()
                {
                    var parameters = _method.Parameters;
                    var derivativeIndex = _index + 1;
                    var current = _states.Value.State;
                    var previous = _states.GetPreviousValue(1).State;

                    var diff = new double[_method.MaxOrder + 2];
                    var deltmp = new double[_states.Length];

                    // Calculate the tolerance
                    var volttol =
                        parameters.AbsoluteTolerance + parameters.RelativeTolerance * Math.Max(Math.Abs(current[derivativeIndex]), Math.Abs(previous[derivativeIndex]));
                    var chargetol = Math.Max(Math.Abs(current[_index]), Math.Abs(previous[_index]));
                    chargetol = parameters.RelativeTolerance * Math.Max(chargetol, parameters.ChargeTolerance) / _states.Value.Delta;
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
                            deltmp[i] = deltmp[i + 1] + _states.GetPreviousValue(i).Delta;
                    }

                    // Calculate the new timestep
                    double factor = double.NaN;
                    switch (_method.Order)
                    {
                        case 1:
                            factor = 0.5;
                            break;
                        case 2:
                            factor = 0.2222222222;
                            break;
                        case 3:
                            factor = 0.1363636364;
                            break;
                        case 4:
                            factor = 0.096;
                            break;
                        case 5:
                            factor = 0.07299270073;
                            break;
                        case 6:
                            factor = 0.05830903790;
                            break;
                    }

                    var del = parameters.TrTol * tol / Math.Max(parameters.AbsoluteTolerance, factor * Math.Abs(diff[0]));
                    if (_method.Order == 2)
                        del = Math.Sqrt(del);
                    else if (_method.Order > 2)
                        del = Math.Exp(Math.Log(del) / _method.Order);
                    return del;
                }
            }
        }
    }
}
