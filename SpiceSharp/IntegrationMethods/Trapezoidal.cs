using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Trapezoidal rule implementation.
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
        public Trapezoidal(IntegrationConfiguration config = null)
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
        /// <param name="ckt">The circuit</param>
        public override void Predict(Circuit ckt)
        {
            // Get the state
            var state = ckt.State;

            // Predict a solution
            double a, b;
            switch (Order)
            {
                case 1:
                    // Divided difference approach
                    for (int i = 0; i < Solutions[0].Length; i++)
                    {
                        double dd0 = (Solutions[0][i] - Solutions[1][i]) / DeltaOld[1];
                        Prediction[i] = Solutions[0][i] + DeltaOld[0] * dd0;
                    }
                    break;

                case 2:
                    // Adams-Bashforth method (second order for variable timesteps)
                    b = -DeltaOld[0] / (2.0 * DeltaOld[1]);
                    a = 1 - b;
                    for (int i = 0; i < Solutions[0].Length; i++)
                    {
                        double dd0 = (Solutions[0][i] - Solutions[1][i]) / DeltaOld[1];
                        double dd1 = (Solutions[1][i] - Solutions[2][i]) / DeltaOld[2];
                        Prediction[i] = Solutions[0][i] + (b * dd1 + a * dd0) * DeltaOld[0];
                    }
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }
        }

        /// <summary>
        /// Truncate the timestep
        /// Uses the Local Truncation Error (LTE) to calculate an approximate timestep.
        /// The method is slightly different from the original Spice 3f5 version.
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public override double TruncateNodes(Circuit ckt)
        {
            // Get the state
            var state = ckt.State;
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
                        index = node.Index;

                        // Milne's estimate for the second-order derivative using a Forward Euler predictor and Backward Euler corrector
                        diff = state.Solution[index] - Prediction[index];
                        if (diff != 0.0)
                        {
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
                        index = node.Index;

                        // Milne's estimate for the third-order derivative using an Adams-Bashforth predictor and Trapezoidal corrector
                        diff = state.Solution[index] - Prediction[index];
                        double deriv = DeltaOld[1] / DeltaOld[0];
                        deriv = diff * 4.0 / (1 + deriv * deriv);

                        if (deriv != 0.0)
                        {
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

        /// <summary>
        /// Control local truncation error on a state variable
        /// </summary>
        /// <param name="qcap">Index</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Terr(int qcap, Circuit ckt, ref double timeStep)
        {
            var state = ckt.State;
            var config = ckt.Simulation.Config ?? Simulations.SimulationConfiguration.Default;
            int ccap = qcap + 1;

            double[] diff = new double[state.States.Length];
            double[] deltmp = new double[DeltaOld.Length];

            // Calculate the tolerance
            double volttol = config.AbsTol + config.RelTol * Math.Max(Math.Abs(state.States[0][ccap]), Math.Abs(state.States[1][ccap]));
            double chargetol = Math.Max(Math.Abs(state.States[0][qcap]), Math.Abs(state.States[1][qcap]));
            chargetol = config.RelTol * Math.Max(chargetol, config.ChgTol) / Delta;
            double tol = Math.Max(volttol, chargetol);

            // Now divided differences
            for (int i = 0; i < diff.Length; i++)
                diff[i] = state.States[i][qcap];
            for (int i = 0; i < deltmp.Length; i++)
                deltmp[i] = DeltaOld[i];
            int j = Order;
            while (true)
            {
                for (int i = 0; i <= j; i++)
                    diff[i] = (diff[i] - diff[i + 1]) / deltmp[i];
                if (--j < 0)
                    break;
                for (int i = 0; i <= j; i++)
                    deltmp[i] = deltmp[i + 1] + DeltaOld[i];
            }

            // Calculate the new timestep
            double factor;
            switch (Order)
            {
                case 1: factor = 0.5; break;
                case 2: factor = 0.0833333333; break;
                default: throw new CircuitException($"Invalid order {Order}");
            }
            double del = Config.TrTol * tol / Math.Max(config.AbsTol, factor * Math.Abs(diff[0]));
            if (Order == 2)
                del = Math.Sqrt(del);
            else if (Order > 2)
                del = Math.Exp(Math.Log(del) / Order);

            // Return the timestep
            timeStep = Math.Min(timeStep, del);
        }
    }
}
