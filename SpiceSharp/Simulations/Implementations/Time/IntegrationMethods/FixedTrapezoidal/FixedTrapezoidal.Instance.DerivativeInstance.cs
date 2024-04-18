using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class FixedTrapezoidal
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="IDerivative"/> for <see cref="FixedTrapezoidal"/>.
            /// </summary>
            /// <seealso cref="IDerivative"/>
            protected class DerivativeInstance : IDerivative
            {
                private readonly int _index;
                private readonly Instance _method;
                private readonly IHistory<IVector<double>> _states;

                /// <inheritdoc />
                public double Derivative => _states.Value[_index + 1];

                /// <inheritdoc />
                public double Value
                {
                    get => _states.Value[_index];
                    set => _states.Value[_index] = value;
                }

                /// <summary>
                /// Creates a new <see cref="DerivativeInstance"/>.
                /// </summary>
                /// <param name="method">The integration method.</param>
                /// <param name="index">The index.</param>
                public DerivativeInstance(Instance method, int index)
                {
                    _method = method.ThrowIfNull(nameof(method));
                    _states = _method._states;
                    _index = index;
                }

                /// <inheritdoc/>
                public JacobianInfo GetContributions(double coefficient, double currentValue)
                {
                    double g = _method.Slope * coefficient;
                    return new JacobianInfo(
                        g,
                        Derivative - g * currentValue);
                }

                /// <inheritdoc/>
                public JacobianInfo GetContributions(double coefficient)
                {
                    double h = _method.Slope;
                    var s = _states.Value;
                    return new JacobianInfo(
                        h * coefficient,
                        s[_index + 1] - h * s[_index]);
                }

                /// <inheritdoc/>
                public double GetPreviousDerivative(int index)
                    => _states.GetPreviousValue(index)[_index + 1];

                /// <inheritdoc/>
                public double GetPreviousValue(int index)
                    => _states.GetPreviousValue(index)[_index];

                /// <inheritdoc />
                public void Derive()
                {
                    int derivativeIndex = _index + 1;
                    var current = _states.Value;
                    var previous = _states.GetPreviousValue(1);

                    switch (_method.Order)
                    {
                        case 1:
                            current[derivativeIndex] = _method._ag0 * (current[_index] - previous[_index]);
                            break;

                        case 2:
                            current[derivativeIndex] = -previous[derivativeIndex] * _method._ag1 +
                                _method._ag0 * (current[_index] - previous[_index]);
                            break;
                    }
                }
            }
        }
    }
}
