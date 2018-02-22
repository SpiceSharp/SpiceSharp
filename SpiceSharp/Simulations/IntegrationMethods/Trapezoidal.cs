using System;
using System.Collections.ObjectModel;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.NewSparse;

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
        double[] ag = new double[2];

        /// <summary>
        /// Constructor
        /// </summary>
        public Trapezoidal()
            : base(new IntegrationParameters(), 2)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Trapezoidal(IntegrationParameters config)
            : base(config, 2)
        {
        }
        
        /// <summary>
        /// Initialize the trapezoidal integration method
        /// </summary>
        /// <param name="behaviors">Truncation behaviors</param>
        public override void Initialize(Collection<TransientBehavior> behaviors)
        {
            base.Initialize(behaviors);

            ag = new double[MaxOrder];
            for (int i = 0; i < MaxOrder; i++)
                ag[i] = 0.0;
        }

        /// <summary>
        /// Integrate a variable at a specific index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public override void Integrate(History<Vector<double>> history, int index)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));
            int derivativeIndex = index + 1;
            if (index < 0 || derivativeIndex > history.Current.Length)
                throw new CircuitException("Invalid state index {0}".FormatString(index));

            var current = history.Current;
            var previous = history[1];

            switch (Order)
            {
                case 1:
                    current[derivativeIndex] = ag[0] * current[index] + ag[1] * previous[index];
                    break;

                case 2:
                    current[derivativeIndex] = -previous[derivativeIndex] * ag[1] + ag[0] * (current[index] - previous[index]);
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }
        }

        /// <summary>
        /// Predict a new solution based on the previous ones
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Predict(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Predict a solution
            double a, b;
            switch (Order)
            {
                case 1:
                    // Divided difference approach
                    for (int i = 1; i <= Solutions[0].Length; i++)
                    {
                        double dd0 = (Solutions[0][i] - Solutions[1][i]) / DeltaOld[1];
                        Prediction[i] = Solutions[0][i] + DeltaOld[0] * dd0;
                    }
                    break;

                case 2:
                    // Adams-Bashforth method (second order for variable timesteps)
                    b = -DeltaOld[0] / (2.0 * DeltaOld[1]);
                    a = 1 - b;
                    for (int i = 1; i <= Solutions[0].Length; i++)
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
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <returns></returns>
        protected override void TruncateNodes(object sender, TruncationEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            // Get the state
            var simulation = args.Simulation;
            var state = simulation.RealState;
            double tol, diff, tmp;
            double timetemp = Double.PositiveInfinity;
            var nodes = simulation.Circuit.Nodes;
            int index = 0;

            // In my opinion, the original Spice method is kind of bugged and can be much better...
            switch (Order)
            {
                case 1:
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node.UnknownType != Node.NodeType.Voltage)
                            continue;
                        index = node.Index;

                        // Milne's estimate for the second-order derivative using a Forward Euler predictor and Backward Euler corrector
                        diff = state.Solution[index] - Prediction[index];
                        if (diff != 0.0)
                        {
                            tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * BaseParameters.LteRelativeTolerance + BaseParameters.LteAbsoluteTolerance;
                            tmp = DeltaOld[0] * Math.Sqrt(Math.Abs(2.0 * BaseParameters.TruncationTolerance * tol / diff));
                            timetemp = Math.Min(timetemp, tmp);
                        }
                    }
                    break;

                case 2:
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node.UnknownType != Node.NodeType.Voltage)
                            continue;
                        index = node.Index;

                        // Milne's estimate for the third-order derivative using an Adams-Bashforth predictor and Trapezoidal corrector
                        diff = state.Solution[index] - Prediction[index];
                        double deriv = DeltaOld[1] / DeltaOld[0];
                        deriv = diff * 4.0 / (1 + deriv * deriv);

                        if (deriv != 0.0)
                        {
                            tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * BaseParameters.LteRelativeTolerance + BaseParameters.LteAbsoluteTolerance;
                            tmp = DeltaOld[0] * Math.Pow(Math.Abs(12.0 * BaseParameters.TruncationTolerance * tol / deriv), 1.0 / 3.0);
                            timetemp = Math.Min(timetemp, tmp);
                        }
                    }
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }

            // Get the minimum timestep
            args.Delta = timetemp;
        }

        /// <summary>
        /// Compute the coefficients for Trapezoidal integration
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void ComputeCoefficients(TimeSimulation simulation)
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
                    throw new CircuitException("Invalid order {0}".FormatString(Order));
            }

            // Store the derivative w.r.t. the current timestep
            Slope = ag[0];
        }

        /// <summary>
        /// Calculate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <param name="history">History</param>
        /// <param name="index">Index</param>
        /// <param name="timestep">Timestep</param>
        public override void LocalTruncateError(History<Vector<double>> history, int index, ref double timestep)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));
            int derivativeIndex = index + 1;
            if (index < 0 || derivativeIndex > history.Current.Length)
                throw new CircuitException("Invalid state index {0}".FormatString(index));
            var current = history.Current;
            var previous = history[1];

            double[] diff = new double[MaxOrder + 2];
            double[] deltmp = new double[DeltaOld.Length];

            // Calculate the tolerance
            // Note: These need to be available in the integration method configuration, defaults are used for now to avoid too much changes
            double volttol = 1e-12 + 1e-3 * Math.Max(Math.Abs(current[derivativeIndex]), Math.Abs(previous[derivativeIndex]));
            double chargetol = Math.Max(Math.Abs(current[index]), Math.Abs(previous[index]));
            chargetol = 1e-3 * Math.Max(chargetol, 1e-14) / Delta;
            double tol = Math.Max(volttol, chargetol);

            // Now divided differences
            int j = 0;
            foreach (var states in history)
                diff[j++] = states[index];
            for (int i = 0; i < deltmp.Length; i++)
                deltmp[i] = DeltaOld[i];
            j = Order;
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
                default: throw new CircuitException("Invalid order {0}".FormatString(Order));
            }
            double del = BaseParameters.TruncationTolerance * tol / Math.Max(1e-12, factor * Math.Abs(diff[0]));
            if (Order == 2)
                del = Math.Sqrt(del);
            else if (Order > 2)
                del = Math.Exp(Math.Log(del) / Order);

            // Return the timestep
            timestep = Math.Min(timestep, del);
        }
    }
}
