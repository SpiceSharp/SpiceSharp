using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class FixedTrapezoidal
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="IIntegral"/> for <see cref="FixedTrapezoidal"/>.
            /// </summary>
            /// <seealso cref="IIntegral"/>
            protected class IntegralInstance : IIntegral
            {
                private readonly int _index;
                private readonly Instance _method;
                private readonly IHistory<IVector<double>> _states;

                /// <inheritdoc />
                public double Derivative
                {
                    get => _states.Value[_index];
                    set => _states.Value[_index] = value;
                }

                /// <inheritdoc />
                public double Value => _states.Value[_index + 1];

                /// <summary>
                /// Creates a new <see cref="IntegralInstance"/>.
                /// </summary>
                /// <param name="method">The integration method.</param>
                /// <param name="index">The index in the vector.</param>
                public IntegralInstance(Instance method, int index)
                {
                    _method = method.ThrowIfNull(nameof(method));
                    _states = _method._states;
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
                    var s = _states.Value;
                    return new JacobianInfo(
                        h * coefficient,
                        s[_index + 1] - h * s[_index]);
                }

                /// <inheritdoc/>
                public double GetPreviousValue(int index)
                    => _states.GetPreviousValue(index)[_index + 1];

                /// <inheritdoc/>
                public double GetPreviousDerivative(int index)
                    => _states.GetPreviousValue(index)[_index];

                /// <inheritdoc/>
                public void Integrate()
                {
                    int integratedIndex = _index + 1;
                    var current = _states.Value;
                    var previous = _states.GetPreviousValue(1);

                    switch (_method.Order)
                    {
                        case 1:
                            current[integratedIndex] = (current[_index] - _method._ag1 * previous[integratedIndex]) / _method._ag0;
                            break;

                        case 2:
                            current[integratedIndex] = previous[integratedIndex] + (current[_index] + _method._ag1 * previous[_index]) / _method._ag0;
                            break;
                    }
                }
            }
        }
    }
}
