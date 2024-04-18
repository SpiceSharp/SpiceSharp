using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class FixedEuler
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="IIntegral"/> for <see cref="FixedEuler"/>.
            /// </summary>
            /// <seealso cref="IIntegral"/>
            protected class IntegralInstance : IIntegral
            {
                private readonly int _index;
                private readonly Instance _method;
                private readonly IHistory<IVector<double>> _states;

                /// <inheritdoc/>
                public double Value => _states.Value[_index + 1];

                /// <inheritdoc/>
                public double Derivative
                {
                    get => _states.Value[_index];
                    set => _states.Value[_index] = value;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="IntegralInstance"/> class.
                /// </summary>
                /// <param name="method">The method.</param>
                /// <param name="index">The index.</param>
                public IntegralInstance(Instance method, int index)
                {
                    _method = method.ThrowIfNull(nameof(method));
                    _states = method._states;
                    _index = index;
                }

                /// <inheritdoc/>
                public JacobianInfo GetContributions(double coefficient, double currentValue)
                {
                    double g = coefficient / _method.Slope;
                    return new JacobianInfo(g, Value - g * currentValue);
                }

                /// <inheritdoc/>
                public JacobianInfo GetContributions(double coefficient)
                {
                    double h = 1 / _method.Slope;
                    var s = _states.Value;
                    return new JacobianInfo(h * coefficient, s[_index + 1] - h * s[_index]);
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
                    var current = _states.Value;
                    var previous = _states.GetPreviousValue(1);
                    current[_index + 1] = previous[_index + 1] + current[_index] / _method.Slope;
                }
            }
        }
    }
}
