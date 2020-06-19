using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class FixedEuler
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="IDerivative"/> for <see cref="FixedEuler"/>.
            /// </summary>
            /// <seealso cref="IDerivative" />
            protected class DerivativeInstance : IDerivative
            {
                private readonly int _index;
                private readonly Instance _method;
                private readonly IHistory<IVector<double>> _states;

                /// <summary>
                /// Gets or sets the current value.
                /// </summary>
                /// <value>
                /// The value.
                /// </value>
                public double Value
                {
                    get => _states.Value[_index];
                    set => _states.Value[_index] = value;
                }

                /// <summary>
                /// Gets the current derivative.
                /// </summary>
                /// <value>
                /// The derivative.
                /// </value>
                public double Derivative => _states.Value[_index + 1];

                /// <summary>
                /// Initializes a new instance of the <see cref="DerivativeInstance"/> class.
                /// </summary>
                /// <param name="method">The method.</param>
                /// <param name="index">The index.</param>
                public DerivativeInstance(Instance method, int index)
                {
                    _method = method.ThrowIfNull(nameof(method));
                    _states = method._states;
                    _index = index;
                }

                /// <summary>
                /// Gets the Y-matrix value and Rhs-vector contributions for the derived quantity.
                /// </summary>
                /// <param name="coefficient">The coefficient of the quantity that is derived.</param>
                /// <param name="currentValue">The current value of the derived state.</param>
                /// <returns>
                /// The information for filling in the Y-matrix and Rhs-vector.
                /// </returns>
                public JacobianInfo GetContributions(double coefficient, double currentValue)
                {
                    var g = _method.Slope * coefficient;
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
                    var s = _states.Value;
                    return new JacobianInfo(
                        h * coefficient,
                        s[_index + 1] - h * s[_index]);
                }

                /// <summary>
                /// Gets a previous derivative. An index of 0 indicates the current value.
                /// </summary>
                /// <param name="index">The number of points to go back in time.</param>
                /// <returns>
                /// The previous derivative.
                /// </returns>
                public double GetPreviousDerivative(int index)
                    => _states.GetPreviousValue(index)[_index + 1];

                /// <summary>
                /// Gets a previous value. An index of 0 indicates the current value.
                /// </summary>
                /// <param name="index">The index.</param>
                /// <returns>
                /// The previous value.
                /// </returns>
                public double GetPreviousValue(int index)
                    => _states.GetPreviousValue(index)[_index];

                /// <summary>
                /// Integrates the state (calculates the derivative).
                /// </summary>
                public void Integrate()
                {
                    var current = _states.Value;
                    var previous = _states.GetPreviousValue(1);
                    current[_index + 1] = (current[_index] - previous[_index]) * _method.Slope;
                }
            }
        }
    }
}
