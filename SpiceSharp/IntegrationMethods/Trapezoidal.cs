using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private double h = 0.0;

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
        /// Integrate a state variable
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="index">The state index to integrate</param>
        /// <param name="cap">The capacitance</param>
        /// <returns></returns>
        public override Result Integrate(CircuitState state, int index, double cap)
        {
            switch (Order)
            {
                case 1:
                    state.States[0][index + 1] = h * (state.States[0][index] - state.States[1][index]);
                    break;

                case 2:
                    state.States[0][index + 1] = h * (state.States[0][index] - state.States[1][index]) - state.States[1][index + 1];
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }

            // Create the returned object
            Result result = new Result();
            result.Ceq = state.States[0][index + 1] - h * state.States[0][index];
            result.Geq = h * cap;
            return result;
        }

        /// <summary>
        /// Predict a new solution based on the previous ones
        /// </summary>
        /// <param name="ckt"></param>
        public override void Predict(Circuit ckt)
        {
            // Get the state
            var state = ckt.State;

            // Predict a solution
            double a, b;
            Vector<double> dd0, dd1;
            switch (Order)
            {
                case 1:
                    dd0 = (Solutions[0] - Solutions[1]) / DeltaOld[1];
                    Prediction = Solutions[0] + DeltaOld[0] * dd0;
                    Prediction.CopyTo(state.Solution);
                    break;

                case 2:
                    // b = -DeltaOld[0] / (2.0 * DeltaOld[1]);
                    b = (DeltaOld[0] + DeltaOld[1]) / (DeltaOld[1] + DeltaOld[2]);
                    // a = 1 - b;
                    a = 1 + b;
                    dd0 = (Solutions[0] - Solutions[1]) / DeltaOld[1];
                    dd1 = (Solutions[1] - Solutions[2]) / DeltaOld[2];
                    Prediction = Solutions[0] + (a * dd0 - b * dd1) * DeltaOld[0];
                    Prediction.CopyTo(state.Solution);
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
            var state = ckt.State;

            double tol, diff, tmp;
            double timetemp = Double.PositiveInfinity;
            int rows = ckt.Nodes.Count;
            int index = 0;

            switch (Order)
            {
                case 1:
                    for (int i = 0; i < ckt.Nodes.Count; i++)
                    {
                        var node = ckt.Nodes[i];
                        index = node.Index;
                        tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * Config.LteRelTol + Config.LteAbsTol;
                        if (node.Type != CircuitNode.NodeType.Voltage)
                            continue;
                        diff = state.Solution[index] - Prediction[index];
                        if (diff != 0)
                        {
                            tmp = 2.0 * Config.TrTol * tol / diff * DeltaOld[0] * (DeltaOld[0] + DeltaOld[1]);
                            tmp = Math.Sqrt(Math.Abs(tmp));
                            timetemp = Math.Min(timetemp, tmp);
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < ckt.Nodes.Count; i++)
                    {
                        var node = ckt.Nodes[i];
                        index = node.Index;
                        tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * Config.LteRelTol + Config.LteAbsTol;
                        if (node.Type != CircuitNode.NodeType.Voltage)
                            continue;
                        diff = state.Solution[index] - Prediction[index];
                        if (diff != 0)
                        {
                            tmp = 12.0 * Config.TrTol * tol / diff * DeltaOld[0] * (DeltaOld[0] + DeltaOld[1]) * (DeltaOld[0] + DeltaOld[1] + DeltaOld[2]);
                            tmp = Math.Pow(Math.Abs(tmp), 1.0 / 3.0);
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
            switch (Order)
            {
                case 1:
                    h = 1.0 / Delta;
                    break;

                case 2:
                    h = 2.0 / Delta;
                    break;
            }

            // Store the derivation
            Slope = h;
        }
    }
}
