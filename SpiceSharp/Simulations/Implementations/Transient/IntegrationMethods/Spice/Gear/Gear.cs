using System;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A class that implements the Gear integration method.
    /// </summary>
    /// <seealso cref="SpiceIntegrationMethod" />
    public partial class Gear : SpiceIntegrationMethod
    {
        /// <summary>
        /// Gets the integration coefficients.
        /// </summary>
        protected DenseVector<double> Coefficients { get; } = new DenseVector<double>(7);

        /// <summary>
        /// Gets the prediction coefficients.
        /// </summary>
        protected DenseVector<double> PredictionCoefficients { get; } = new DenseVector<double>(7);

        /// <summary>
        /// Matrix used to solve the integration coefficients.
        /// </summary>
        protected DenseRealSolver<DenseMatrix<double>, DenseVector<double>> Solver { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gear"/> class.
        /// </summary>
        public Gear()
            : base(6)
        {
            Solver = new DenseRealSolver<DenseMatrix<double>, DenseVector<double>>(
                new DenseMatrix<double>(7),
                new DenseVector<double>(7)
                );
        }

        /// <summary>
        /// Initializes the integration method.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Initialize(TimeSimulation simulation)
        {
            base.Initialize(simulation);

            // Reset all coefficients
            for (var i = 1; i <= Coefficients.Length; i++)
            {
                Coefficients[i] = 0.0;
                PredictionCoefficients[i] = 0.0;
            }
        }

        /// <summary>
        /// Destroys the integration method.
        /// </summary>
        public override void Unsetup()
        {
            base.Unsetup();

            // Reset coefficients
            for (var i = 1; i <= Coefficients.Length; i++)
            {
                Coefficients[i] = 0.0;
                PredictionCoefficients[i] = 0.0;
            }
        }

        /// <summary>
        /// Predicts a solution
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        protected override void Predict(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Use the previous solutions to predict a new one
            for (var i = 0; i <= Prediction.Length; i++)
            {
                Prediction[i] = 0.0;
                for (var k = 0; k <= Order; k++)
                {
                    Prediction[i] += PredictionCoefficients[k + 1] * IntegrationStates[k + 1].Solution[i];
                }
            }
        }

        /// <summary>
        /// Truncates the timestep using nodes.
        /// </summary>
        /// <param name="sender">The sender (integration method).</param>
        /// <param name="args">The <see cref="SpiceSharp.IntegrationMethods.TruncateEvaluateEventArgs" /> instance containing the event data.</param>
        protected override void TruncateNodes(object sender, TruncateEvaluateEventArgs args)
        {
            args.ThrowIfNull(nameof(args));

            // Get the state
            var timetmp = double.PositiveInfinity;
            var nodes = args.Simulation.Variables;

            var delsum = 0.0;
            for (var i = 0; i <= Order; i++)
                delsum += IntegrationStates[i].Delta;

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var index = node.Index;
                var tol = Math.Max(Math.Abs(BiasingState.Solution[index]), Math.Abs(Prediction[index])) * RelTol + AbsTol;
                var diff = BiasingState.Solution[index] - Prediction[i];

                if (!diff.Equals(0.0))
                {
                    var tmp = tol * TrTol * delsum / (diff * IntegrationStates[0].Delta);
                    tmp = Math.Abs(tmp);
                    switch (Order)
                    {
                        case 0: break;
                        case 1:
                            tmp = Math.Sqrt(tmp);
                            break;
                        default:
                            tmp = Math.Exp(Math.Log(tmp) / (Order + 1));
                            break;
                    }

                    tmp *= IntegrationStates[0].Delta;
                    timetmp = Math.Min(timetmp, tmp);
                }

                args.Delta = timetmp;
            }
        }

        /// <summary>
        /// Computes the integration coefficients.
        /// </summary>
        protected override void ComputeCoefficients()
        {
            var delta = IntegrationStates[0].Delta;

            // Set up the matrix
            int n = Order + 1;
            for (var i = 1; i <= n; i++)
                Solver[i] = 0.0;
            Solver[2] = -1 / delta;
            for (var i = 1; i <= n; i++)
                Solver[1, i] = 1.0;
            for (var i = 2; i <= n; i++)
                Solver[i, 1] = 0.0;

            double arg = 0.0, arg1;
            for (var i = 2; i <= n; i++)
            {
                arg += IntegrationStates[i - 2].Delta;
                arg1 = 1.0;
                for (var j = 2; j <= n; j++)
                {
                    arg1 *= arg / delta;
                    Solver[j, i] = arg1;
                }
            }
            Solver.Factor(n);
            Solver.Solve(Coefficients, n);

            // Predictor calculations
            for (var i = 2; i <= n; i++)
                Solver[i] = 0.0;
            Solver[1] = 1.0;
            for (var i = 1; i <= n; i++)
                Solver[1, i] = 1.0;
            arg = 0.0;
            for (var i = 1; i <= n; i++)
            {
                arg += IntegrationStates[i - 1].Delta;
                arg1 = 1.0;
                for (var j = 2; j <= n; j++)
                {
                    arg1 *= arg / delta;
                    Solver[j, i] = arg1;
                }
            }
            Solver.Factor(n);
            Solver.Solve(PredictionCoefficients, n);

            // Store the derivative w.r.t. the current timestep
            Slope = Coefficients[1];
        }

        /// <summary>
        /// Produces a derivative.
        /// </summary>
        /// <returns>
        /// A <see cref="SpiceSharp.IntegrationMethods.StateDerivative" /> that can be used with this integration method.
        /// </returns>
        protected override StateDerivative ProduceDerivative() => new GearStateDerivative(this);
    }
}
