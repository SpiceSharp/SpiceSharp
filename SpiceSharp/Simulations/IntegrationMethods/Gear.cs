using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Trapezoidal rule implementation.
    /// </summary>
    public class Gear : IntegrationMethod
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private double[] _ag = new double[6];
        private double[] _agp = new double[6];
        private DenseMatrix<double> _mat = new DenseMatrix<double>(8);

        /// <summary>
        /// Constructor
        /// </summary>
        public Gear()
            : base(new IntegrationParameters(), 6)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Gear(IntegrationParameters config)
            : base(config, 6)
        {
        }

        /// <summary>
        /// Initialize the Gear integration method
        /// </summary>
        /// <param name="behaviors">Truncation behaviors</param>
        public override void Initialize(BehaviorList<BaseTransientBehavior> behaviors)
        {
            base.Initialize(behaviors);

            _ag = new double[MaxOrder];
            for (int i = 0; i < MaxOrder; i++)
                _ag[i] = 0.0;
        }

        /// <summary>
        /// Integrate a variable at a specific index
        /// </summary>
        /// <param name="history">History</param>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public override void Integrate(History<Vector<double>> history, int index)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));
            var derivativeIndex = index + 1;
            if (index < 0 || derivativeIndex > history.Current.Length)
                throw new CircuitException("Invalid state index {0}".FormatString(index));

            var current = history.Current;
            current[derivativeIndex] = 0.0;
            if (Order < 0 || Order > MaxOrder)
                throw new CircuitException("Invalid order");
            for (var i = 0; i <= Order; i++)
                current[derivativeIndex] += _ag[i] * history[i][index];
        }

        /// <summary>
        /// Predict a new solution based on the previous ones
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Predict(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            for (var i = 0; i <= Solutions[0].Length; i++)
            {
                Prediction[i] = 0.0;
                for (var k = 0; k <= Order; k++)
                {
                    Prediction[i] += _agp[k] * Solutions[k][i];
                }
            }
        }

        /// <summary>
        /// Truncate the timestep
        /// Uses the Local Truncation Error (LTE) to calculate an approximate timestep.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <returns></returns>
        protected override void TruncateNodes(object sender, TruncationEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var simulation = args.Simulation;
            var nodes = simulation.Nodes;
            var state = simulation.RealState;
            double delsum = 0;
            var timetmp = double.PositiveInfinity;
            for (var i = 0; i <= Order; i++)
                delsum += DeltaOld[i];

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var index = node.Index;
                var tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) *
                             BaseParameters.LteRelativeTolerance + BaseParameters.LteAbsoluteTolerance;
                var diff = state.Solution[index] - Prediction[i];

                if (!diff.Equals(0.0))
                {
                    var tmp = tol * BaseParameters.TruncationTolerance * delsum / (diff * Delta);
                    tmp = Math.Abs(tmp);
                    switch (Order)
                    {
                        case 0: break;
                        case 1: tmp = Math.Sqrt(tmp);
                            break;
                        default: tmp = Math.Exp(Math.Log(tmp) / (Order + 1));
                            break;
                    }

                    tmp *= Delta;
                    timetmp = Math.Min(timetmp, tmp);
                }
            }

            // Store the minimum timestep
            args.Delta = timetmp;
        }

        /// <summary>
        /// Compute the coefficients for Trapezoidal integration
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void ComputeCoefficients(TimeSimulation simulation)
        {
            for (var i = 0; i < _ag.Length; i++)
                _ag[i] = 0.0;
            _ag[1] = -1.0 / Delta;

            // First, set up the matrix
            double arg = 0, arg1;
            for (var i = 0; i <= Order; i++)
                _mat[0, i] = 1.0;
            for (var i = 1; i <= Order; i++)
                _mat[i, 0] = 0.0;

            for (var i = 1; i <= Order; i++)
            {
                arg += DeltaOld[i - 1];
                arg1 = 1.0;
                for (var j = 1; j <= Order; j++)
                {
                    arg1 *= arg / Delta;
                    _mat[j, i] = arg1;
                }
            }

            // LU decompose
            // The first column is already decomposed!
            for (var i = 1; i <= Order; i++)
            {
                for (var j = i + 1; j <= Order; j++)
                {
                    _mat[j, i] /= _mat[i, i];
                    for (var k = i + 1; k <= Order; k++)
                        _mat[j, k] -= _mat[j, i] * _mat[i, k];
                }
            }

            // Forward substitution
            for (var i = 1; i <= Order; i++)
            {
                for (var j = i + 1; j <= Order; j++)
                    _ag[j] = _ag[j] - _mat[j, i] * _ag[i];
            }

            // Backward substitution
            _ag[Order] /= _mat[Order, Order];
            for (var i = Order - 1; i >= 0; i--)
            {
                for (var j = i + 1; j <= Order; j++)
                    _ag[i] = _ag[i] - _mat[i, j] * _ag[j];
                _ag[i] /= _mat[i, i];
            }

            // Predictor calculations
            for (var i = 1; i < _agp.Length; i++)
                _agp[i] = 0.0;
            _agp[0] = 1.0;
            for (var i = 0; i <= Order; i++)
                _mat[0, i] = 1.0;
            arg = 0.0;
            for (var i = 0; i <= Order; i++)
            {
                arg += DeltaOld[i];
                arg1 = 1.0;
                for (var j = 1; j <= Order; j++)
                {
                    arg1 *= arg / Delta;
                    _mat[j, i] = arg1;
                }
            }

            // LU decomposition
            for (var i = 0; i <= Order; i++)
            {
                for (var j = i + 1; j <= Order; j++)
                {
                    _mat[j, i] /= _mat[i, i];
                    for (var k = i + 1; k <= Order; k++)
                        _mat[j, k] -= _mat[j, i] * _mat[i, k];
                }
            }

            // Forward substitution
            for (var i = 0; i <= Order; i++)
            {
                for (var j = i + 1; j <= Order; j++)
                    _agp[j] -= _mat[j, i] * _agp[i];
            }

            // Backward substitution
            _agp[Order] /= _mat[Order, Order];
            for (var i = Order - 1; i >= 0; i--)
            {
                for (var j = i + 1; j <= Order; j++)
                    _agp[i] -= _mat[i, j] * _agp[j];
                _agp[i] /= _mat[i, i];
            }

            // Store the derivative w.r.t. the current timestep
            Slope = _ag[0];
        }

        /// <summary>
        /// Calculate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <param name="history">History</param>
        /// <param name="index">Index</param>
        /// <returns>The timestep that satisfies the LTE</returns>
        public override double LocalTruncateError(History<Vector<double>> history, int index)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));
            var derivativeIndex = index + 1;
            if (index < 0 || derivativeIndex > history.Current.Length)
                throw new CircuitException("Invalid state index {0}".FormatString(index));
            var current = history.Current;
            var previous = history[1];

            var diff = new double[MaxOrder + 2];
            var deltmp = new double[DeltaOld.Length];

            // Calculate the tolerance
            // Note: These need to be available in the integration method configuration, defaults are used for now to avoid too much changes
            var volttol = 1e-12 + 1e-3 * Math.Max(Math.Abs(current[derivativeIndex]), Math.Abs(previous[derivativeIndex]));
            var chargetol = Math.Max(Math.Abs(current[index]), Math.Abs(previous[index]));
            chargetol = 1e-3 * Math.Max(chargetol, 1e-14) / Delta;
            var tol = Math.Max(volttol, chargetol);

            // Now divided differences
            var j = 0;
            foreach (var states in history)
                diff[j++] = states[index];
            for (var i = 0; i < deltmp.Length; i++)
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
                case 1: factor = 0.5;
                    break;
                case 2: factor = 0.2222222222;
                    break;
                case 3: factor = 0.1363636364;
                    break;
                case 4: factor = 0.096;
                    break;
                case 5: factor = 0.07299270073;
                    break;
                case 6: factor = 0.05830903790;
                    break;
                default: throw new CircuitException("Invalid order {0}".FormatString(Order));
            }
            double del = BaseParameters.TruncationTolerance * tol / Math.Max(1e-12, factor * Math.Abs(diff[0]));
            if (Order == 2)
                del = Math.Sqrt(del);
            else if (Order > 2)
                del = Math.Exp(Math.Log(del) / Order);

            // Return the timestep
            return del;
        }
    }
}
