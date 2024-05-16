using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class Trapezoidal
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="IIntegral"/> for <see cref="Trapezoidal"/>.
            /// </summary>
            /// <seealso cref="IIntegral"/>
            /// <seealso cref="ITruncatable"/>
            protected class IntegralInstance : IIntegral, ITruncatable
            {
                private readonly int _index;
                private readonly Instance _method;
                private readonly IHistory<SpiceIntegrationState> _states;

                /// <inheritdoc/>
                public double Derivative
                {
                    get => _states.Value.State[_index];
                    set => _states.Value.State[_index] = value;
                }

                /// <inheritdoc/>
                public double Value => _states.Value.State[_index + 1];

                /// <summary>
                /// Initializes a new instance of the <see cref="IntegralInstance"/> class.
                /// </summary>
                /// <param name="method">The integration method.</param>
                /// <param name="index">The derivative value index.</param>
                public IntegralInstance(Instance method, int index)
                {
                    _method = method.ThrowIfNull(nameof(method));
                    _states = _method.States;
                    _index = index;
                }

                /// <inheritdoc/>
                public JacobianInfo GetContributions(double coefficient, double currentValue)
                {
                    double g = coefficient / _method.Slope;
                    return new JacobianInfo(
                        g,
                        Derivative - g * currentValue);
                }

                /// <inheritdoc/>
                public JacobianInfo GetContributions(double coefficient)
                {
                    double h = 1 / _method.Slope;
                    var s = _states.Value.State;
                    return new JacobianInfo(
                        h * coefficient,
                        s[_index + 1] - h * s[_index]);
                }

                /// <inheritdoc/>
                public double GetPreviousValue(int index)
                    => _states.GetPreviousValue(index).State[_index + 1];

                /// <inheritdoc/>
                public double GetPreviousDerivative(int index)
                    => _states.GetPreviousValue(index).State[_index];

                /// <inheritdoc/>
                public void Integrate()
                {
                    int integratedIndex = _index + 1;
                    var current = _states.Value.State;
                    var previous = _states.GetPreviousValue(1).State;
                    double[] ag = _method.Coefficients;

                    switch (_method.Order)
                    {
                        case 1:
                            current[integratedIndex] = (current[_index] - ag[1] * previous[integratedIndex]) / ag[0];
                            break;

                        case 2:
                            current[integratedIndex] = previous[integratedIndex] + (current[_index] + ag[1] * previous[_index]) / ag[0];
                            break;
                    }
                }

                /// <inheritdoc/>
                public double Truncate()
                {
                    var parameters = _method.Parameters;
                    int derivativeIndex = _index + 1;
                    var current = _states.Value.State;
                    var previous = _states.GetPreviousValue(1).State;

                    double[] diff = new double[_method.MaxOrder + 2];
                    double[] deltmp = new double[_states.Length];

                    // Calculate the tolerance
                    double volttol =
                        parameters.AbsoluteTolerance + parameters.RelativeTolerance * Math.Max(Math.Abs(current[derivativeIndex]), Math.Abs(previous[derivativeIndex]));
                    double chargetol = Math.Max(Math.Abs(current[_index]), Math.Abs(previous[_index]));
                    chargetol = parameters.RelativeTolerance * Math.Max(chargetol, parameters.ChargeTolerance) / _states.Value.Delta;
                    double tol = Math.Max(volttol, chargetol);

                    // Now compute divided differences
                    int j = 0;
                    foreach (var state in _states)
                    {
                        diff[j] = state.State[_index];
                        deltmp[j] = state.Delta;
                        j++;
                    }

                    j = _method.Order;
                    while (true)
                    {
                        for (int i = 0; i <= j; i++)
                            diff[i] = (diff[i] - diff[i + 1]) / deltmp[i];
                        if (--j < 0)
                            break;
                        for (int i = 0; i <= j; i++)
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
                            factor = 0.0833333333;
                            break;
                    }

                    double del = parameters.TrTol * tol / Math.Max(parameters.AbsoluteTolerance, factor * Math.Abs(diff[0]));
                    if (_method.Order == 2)
                        del = Math.Sqrt(del);
                    return del;
                }
            }
        }
    }
}
