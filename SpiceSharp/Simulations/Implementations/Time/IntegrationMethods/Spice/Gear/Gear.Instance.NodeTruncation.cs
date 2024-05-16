using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class Gear
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
                    double timetmp = double.PositiveInfinity;
                    var state = _method.State;
                    var prediction = _method.Prediction;
                    var states = _method.States;

                    double delsum = 0.0;
                    for (int i = 0; i <= _method.Order; i++)
                        delsum += states.GetPreviousValue(i).Delta;

                    foreach (var v in state.Map)
                    {
                        var node = v.Key;
                        int index = v.Value;
                        double tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(prediction[index])) * parameters.RelativeTolerance + parameters.AbsoluteTolerance;
                        double diff = state.Solution[index] - prediction[index];

                        if (!diff.Equals(0.0))
                        {
                            double tmp = tol * parameters.TrTol * delsum / (diff * states.Value.Delta);
                            tmp = Math.Abs(tmp);
                            switch (_method.Order)
                            {
                                case 0: break;
                                case 1:
                                    tmp = Math.Sqrt(tmp);
                                    break;
                                default:
                                    tmp = Math.Exp(Math.Log(tmp) / (_method.Order + 1));
                                    break;
                            }

                            tmp *= states.Value.Delta;
                            timetmp = Math.Min(timetmp, tmp);
                        }
                    }
                    return timetmp;
                }
            }
        }
    }
}
