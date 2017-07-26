using System;
using SpiceSharp.Circuits;
using MathNet.Numerics.LinearAlgebra;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// This class implements the trapezoidal integration method
    /// </summary>
    public class Trapezoidal : IntegrationMethod
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private double[] ag = new double[2];

        /// <summary>
        /// Get the maximum order for the trapezoidal rule
        /// </summary>
        public override int MaxOrder => 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Trapezoidal(Configuration config = null)
            : base(config)
        {
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            ag = new double[MaxOrder];
            for (int i = 0; i < MaxOrder; i++)
                ag[i] = 0.0;
        }

        /// <summary>
        /// Integrate a state variable
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="index">The state index to integrate</param>
        /// <param name="cap">The capacitance</param>
        /// <returns></returns>
        public override Result Integrate(CircuitState state, int qcap, double cap)
        {
            int ccap = qcap + 1;

            switch (Order)
            {
                case 1:
                    state.States[0][ccap] = ag[0] * state.States[0][qcap] + ag[1] * state.States[1][qcap];
                    break;

                case 2:
                    state.States[0][ccap] = -state.States[1][ccap] * ag[1] + ag[0] * (state.States[0][qcap] - state.States[1][qcap]);
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }

            // Create the returned object
            Result result = new Result();
            result.Ceq = state.States[0][ccap] - ag[0] * state.States[0][qcap];
            result.Geq = ag[0] * cap;
            return result;
        }

        /// <summary>
        /// Predict a new solution based on the previous ones
        /// </summary>
        /// <param name="ckt"></param>
        public override void Predict(Circuit ckt)
        {
            // Get the state
            var state = ckt.State.Real;

            // Predict a solution
            double a, b;
            Vector<double> dd0, dd1;
            switch (Order)
            {
                case 1:
                    // Divided difference approach
                    dd0 = (Solutions[0] - Solutions[1]) / DeltaOld[1];
                    Prediction = Solutions[0] + DeltaOld[0] * dd0;
                    Prediction.CopyTo(state.OldSolution);
                    break;

                case 2:
                    // Adams-Bashforth method (second order for variable timesteps)
                    b = -DeltaOld[0] / (2.0 * DeltaOld[1]);
                    a = 1 - b;
                    dd0 = (Solutions[0] - Solutions[1]) / DeltaOld[1];
                    dd1 = (Solutions[1] - Solutions[2]) / DeltaOld[2];
                    Prediction = Solutions[0] + (b * dd1 + a * dd0) * DeltaOld[0];
                    Prediction.CopyTo(state.OldSolution);
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }
        }

        /// <summary>
        /// Truncate the timestep
        /// Uses the Local Truncation Error
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public override double Truncate(Circuit ckt)
        {
            // Get the state
            var state = ckt.State.Real;

            double tol, diff, tmp;
            double timetemp = Double.PositiveInfinity;
            int rows = ckt.Nodes.Count;
            int index = 0;

            // In my opinion, the original Spice method is kind of bugged and can be much better...
            switch (Order)
            {
                case 1:
                    for (int i = 0; i < ckt.Nodes.Count; i++)
                    {
                        var node = ckt.Nodes[i];
                        if (node.Type != CircuitNode.NodeType.Voltage)
                            continue;

                        // Milne's estimate for the second-order derivative using a Forward Euler predictor and Backward Euler corrector
                        diff = state.Solution[index] - Prediction[index];
                        if (diff != 0.0)
                        {
                            index = node.Index;
                            tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * Config.LteRelTol + Config.LteAbsTol;
                            tmp = DeltaOld[0] * Math.Sqrt(Math.Abs(2.0 * Config.TrTol * tol / diff));
                            timetemp = Math.Min(timetemp, tmp);
                        }
                    }
                    break;

                case 2:
                    for (int i = 0; i < ckt.Nodes.Count; i++)
                    {
                        var node = ckt.Nodes[i];
                        if (node.Type != CircuitNode.NodeType.Voltage)
                            continue;

                        // Milne's estimate for the third-order derivative using an Adams-Bashforth predictor and Trapezoidal corrector
                        diff = state.Solution[index] - Prediction[index];
                        double deriv = DeltaOld[1] / DeltaOld[0];
                        deriv = diff * 4.0 / (1 + deriv * deriv);

                        if (deriv != 0.0)
                        {
                            index = node.Index;
                            tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * Config.LteRelTol + Config.LteAbsTol;
                            tmp = DeltaOld[0] * Math.Pow(Math.Abs(12.0 * Config.TrTol * tol / deriv), 1.0 / 3.0);
                            timetemp = Math.Min(timetemp, tmp);
                        }
                    }
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }

            // Get the minimum timestep
            return Math.Min(2.0 * Delta, timetemp);
        }

        /// <summary>
        /// Compute the coefficients for Trapezoidal integration
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void ComputeCoefficients(Circuit ckt)
        {
            // Integration constants
            switch (Order)
            {
                case 1:
                    ag[0] = 1.0 / Delta;
                    ag[1] = -1.0 / Delta;
                    break;

                case 2:
                    ag[0] = 1.0 / Delta / (1.0 - 0.5);
                    ag[1] = 0.5 / (1.0 - 0.5);
                    break;

                default:
                    throw new CircuitException($"Invalid order {Order}");
            }

            // Store the derivative w.r.t. the current timestep
            Slope = ag[0];
        }
    }
}
