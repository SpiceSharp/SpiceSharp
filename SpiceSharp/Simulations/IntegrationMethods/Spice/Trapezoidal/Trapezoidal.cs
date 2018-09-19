using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Trapezoidal rule implementation.
    /// </summary>
    public partial class Trapezoidal : SpiceIntegrationMethod
    {
        /// <summary>
        /// Integration coefficients
        /// </summary>
        protected double[] Coefficients { get; } = new double[2];

        /// <summary>
        /// Constructor
        /// </summary>
        public Trapezoidal()
            : base(2)
        {
        }

        /// <summary>
        /// Initialize the trapezoidal integration method
        /// </summary>
        public override void Initialize(TimeSimulation simulation)
        {
            base.Initialize(simulation);

            // Reset all coefficients
            for (var i = 0; i < MaxOrder; i++)
                Coefficients[i] = 0.0;
        }

        /// <summary>
        /// Unsetup the integration method
        /// </summary>
        public override void Unsetup()
        {
            base.Unsetup();

            // Reset all coefficients
            for (var i = 0; i < MaxOrder; i++)
                Coefficients[i] = 0.0;
        }

        /// <summary>
        /// Predict a new solution based on the previous ones
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        protected override void Predict(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Use the two previous solutions to predict a new one (the one we're about to test)
            var future = IntegrationStates[0];
            var current = IntegrationStates[1];
            var previous = IntegrationStates[2];

            // Predict a solution
            switch (Order)
            {
                case 1:
                    // Divided difference approach
                    for (var i = 1; i <= current.Solution.Length; i++)
                    {
                        var dd0 = (current.Solution[i] - previous.Solution[i]) / current.Delta;
                        Prediction[i] = current.Solution[i] + future.Delta * dd0;
                    }
                    break;

                case 2:
                    // Adams-Bashforth method (second order for variable timesteps)
                    var second = IntegrationStates[3];
                    var b = -future.Delta / (2.0 * current.Delta);
                    var a = 1 - b;
                    for (var i = 1; i <= current.Solution.Length; i++)
                    {
                        var dd0 = (current.Solution[i] - previous.Solution[i]) / current.Delta;
                        var dd1 = (previous.Solution[i] - second.Solution[i]) / previous.Delta;
                        Prediction[i] = current.Solution[i] + (b * dd1 + a * dd0) * future.Delta;
                    }
                    break;

                default:
                    throw new CircuitException("Invalid order");
            }
        }

        /// <summary>
        /// Truncate the timestep using nodes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected override void TruncateNodes(object sender, TruncateEvaluateEventArgs args)
        {
            // Get the state
            var state = args.Simulation.RealState;
            double tol, diff, tmp;
            var timetemp = double.PositiveInfinity;
            var nodes = args.Simulation.Nodes;
            int index;

            // In my opinion, the original Spice method is kind of bugged and can be much better...
            switch (Order)
            {
                case 1:
                    for (var i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node.UnknownType != VariableType.Voltage)
                            continue;
                        index = node.Index;

                        // Milne's estimate for the second-order derivative using a Forward Euler predictor and Backward Euler corrector
                        diff = state.Solution[index] - Prediction[index];

                        // Avoid division by zero
                        if (!diff.Equals(0.0))
                        {
                            tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * RelTol + AbsTol;
                            tmp = IntegrationStates[0].Delta * Math.Sqrt(Math.Abs(2.0 * TrTol * tol / diff));
                            timetemp = Math.Min(timetemp, tmp);
                        }
                    }
                    break;

                case 2:
                    for (var i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node.UnknownType != VariableType.Voltage)
                            continue;
                        index = node.Index;

                        // Milne's estimate for the third-order derivative using an Adams-Bashforth predictor and Trapezoidal corrector
                        diff = state.Solution[index] - Prediction[index];
                        var deriv = IntegrationStates[1].Delta / IntegrationStates[0].Delta;
                        deriv = diff * 4.0 / (1 + deriv * deriv);

                        // Avoid division by zero
                        if (!deriv.Equals(0.0))
                        {
                            tol = Math.Max(Math.Abs(state.Solution[index]), Math.Abs(Prediction[index])) * RelTol + AbsTol;
                            tmp = IntegrationStates[0].Delta * Math.Pow(Math.Abs(12.0 * TrTol * tol / deriv), 1.0 / 3.0);
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
        protected override void ComputeCoefficients()
        {
            var delta = IntegrationStates[0].Delta;

            // Integration constants
            switch (Order)
            {
                case 1:
                    Coefficients[0] = 1.0 / delta;
                    Coefficients[1] = -1.0 / delta;
                    break;

                case 2:
                    Coefficients[0] = 1.0 / delta / (1.0 - 0.5);
                    Coefficients[1] = 0.5 / (1.0 - 0.5);
                    break;

                default:
                    throw new CircuitException("Invalid order {0}".FormatString(Order));
            }

            // Store the derivative w.r.t. the current timestep
            Slope = Coefficients[0];
        }

        /// <summary>
        /// Produce a derivative for the trapezoidal integration method
        /// </summary>
        /// <returns></returns>
        protected override StateDerivative ProduceDerivative() => new TrapezoidalStateDerivative(this);
    }
}
