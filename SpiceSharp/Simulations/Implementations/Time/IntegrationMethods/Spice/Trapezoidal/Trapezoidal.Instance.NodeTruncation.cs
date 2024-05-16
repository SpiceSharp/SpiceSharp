using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class Trapezoidal
    {
        protected partial class Instance
        {
            /// <summary>
            /// An <see cref="ITruncatable"/> that looks at simulation variables.
            /// </summary>
            /// <seealso cref="ITruncatable" />
            protected class NodeTruncation : ITruncatable
            {
                private readonly Instance _method;

                /// <summary>
                /// Initializes a new instance of the <see cref="NodeTruncation"/> class.
                /// </summary>
                /// <param name="method">The integration method.</param>
                public NodeTruncation(Instance method)
                {
                    _method = method.ThrowIfNull(nameof(method));
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
                    double tol, diff, tmp;
                    double timetemp = double.PositiveInfinity;
                    int index;
                    var state = _method.State;
                    var prediction = _method.Prediction;

                    // In my opinion, the original Spice method is kind of bugged and can be much better...
                    switch (_method.Order)
                    {
                        case 1:
                            foreach (var node in state.Map)
                            {
                                if (node.Key.Unit != Units.Volt)
                                    continue;
                                index = node.Value;

                                // Milne's estimate for the second-order derivative using a Forward Euler predictor and Backward Euler corrector
                                diff = state.Solution[index] - prediction[index];

                                // Avoid division by zero
                                if (!diff.Equals(0.0))
                                {
                                    tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(prediction[index])) * parameters.LteRelTol + parameters.LteAbsTol;
                                    tmp = _method.States.Value.Delta * Math.Sqrt(Math.Abs(2.0 * parameters.TrTol * tol / diff));
                                    timetemp = Math.Min(timetemp, tmp);
                                }
                            }
                            break;

                        case 2:
                            foreach (var node in state.Map)
                            {
                                if (node.Key.Unit != Units.Volt)
                                    continue;
                                index = node.Value;

                                // Milne's estimate for the third-order derivative using an Adams-Bashforth predictor and Trapezoidal corrector
                                diff = state.Solution[index] - prediction[index];
                                double deriv = _method.States.GetPreviousValue(1).Delta / _method.States.Value.Delta;
                                deriv = diff * 4.0 / (1 + deriv * deriv);

                                // Avoid division by zero
                                if (!deriv.Equals(0.0))
                                {
                                    tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(prediction[index])) * parameters.LteRelTol + parameters.LteAbsTol;
                                    tmp = _method.States.Value.Delta * Math.Pow(Math.Abs(12.0 * parameters.TrTol * tol / deriv), 1.0 / 3.0);
                                    timetemp = Math.Min(timetemp, tmp);
                                }
                            }
                            break;
                    }

                    return timetemp;
                }
            }
        }
    }
}
