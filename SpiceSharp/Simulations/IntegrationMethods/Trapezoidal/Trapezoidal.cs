using System;
using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Trapezoidal rule implementation.
    /// </summary>
    public partial class Trapezoidal : IntegrationMethod
    {
        /// <summary>
        /// Integration coefficients
        /// </summary>
        protected double[] Coefficients = new double[2];

        /// <summary>
        /// Transient tolerance
        /// </summary>
        protected double TrTol = 7.0;
        protected double RelTol = 1e-3;
        protected double AbsTol = 1e-6;
        protected double MaxStep = 1e-6;
        protected double MinStep = 0.0;
        protected Vector<double> Prediction { get; private set; }
        private double _saveDelta = 0.0;

        /// <summary>
        /// Gets all states that can be derived
        /// </summary>
        protected List<TrapezoidalStateDerivative> DerivativeStates { get; } = new List<TrapezoidalStateDerivative>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Trapezoidal()
            : base(2)
        {
        }

        /// <summary>
        /// Setup the integration method
        /// </summary>
        /// <param name="simulation">Time simulation</param>
        public override void Setup(TimeSimulation simulation)
        {
            base.Setup(simulation);

            // Find our configurations
            var bp = simulation.ParameterSets.Get<TimeConfiguration>();
            MaxStep = bp.MaxStep;
            MinStep = bp.DeltaMin;
            Breakpoints.SetBreakpoint(bp.InitTime);
            Breakpoints.SetBreakpoint(bp.FinalTime);

            // Turn on prediction
            Prediction = new DenseVector<double>(simulation.RealState.Solver.Order);

            // Add events
            TruncateEvaluate += TruncateStates;
        }

        /// <summary>
        /// Probe for a solution
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="delta">Timestep to try</param>
        public override void Probe(TimeSimulation simulation, double delta)
        {
            base.Probe(simulation, delta);

            ComputeCoefficients();
            Predict(simulation);
        }

        /// <summary>
        /// Initialize the trapezoidal integration method
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Reset all coefficients
            Coefficients = new double[MaxOrder];
            for (var i = 0; i < MaxOrder; i++)
                Coefficients[i] = 0.0;
        }

        /// <summary>
        /// Continue
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="delta">Delta</param>
        public override void Continue(TimeSimulation simulation, ref double delta)
        {
            // Let's first calculate our own continue
            delta = Math.Min(delta, MaxStep);

            // Handle breakpoints
            if (Math.Abs(Time - Breakpoints.First) < MinStep)
            {
                var mt = Math.Min(_saveDelta, Breakpoints.Delta);
                delta = Math.Min(delta, mt / 10.0);
                if (BaseTime.Equals(0.0))
                    delta /= 10.0;
                delta = Math.Max(delta, 2 * MinStep);
            }
            else if (Time + delta >= Breakpoints.First)
            {
                _saveDelta = delta;
                delta = Breakpoints.First - Time;
                Break = true;
            }

            base.Continue(simulation, ref delta);
        }

        /// <summary>
        /// Unsetup the integration method
        /// </summary>
        public override void Unsetup()
        {
            base.Unsetup();
            for (var i = 0; i < MaxOrder; i++)
                Coefficients[i] = 0.0;

            // Remove our event
            TruncateEvaluate -= TruncateStates;
        }

        /// <summary>
        /// Create a state that can be derived by the integration method
        /// </summary>
        /// <returns></returns>
        public override StateDerivative CreateDerivative()
        {
            var tsd = new TrapezoidalStateDerivative(this);
            DerivativeStates.Add(tsd);
            return tsd;
        }

        /// <summary>
        /// Truncate the timestep using states
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected void TruncateStates(object sender, TruncateEvaluateEventArgs args)
        {
            // Truncate!
            var newDelta = args.Delta;
            foreach (var d in DerivativeStates)
                newDelta = Math.Min(newDelta, d.Truncate());

            if (newDelta > 0.9 * IntegrationStates[0].Delta)
            {
                if (Order == 1)
                {
                    args.Order = 2;

                    // Try truncation again
                    newDelta = args.Delta;
                    foreach (var d in DerivativeStates)
                        newDelta = Math.Min(newDelta, d.Truncate());

                    if (newDelta <= 1.05 * IntegrationStates[0].Delta)
                        args.Order = 1;
                }
            }
            else
            {
                args.Accepted = false;
                args.Order = 1;
            }

            args.Delta = newDelta;
        }

        /// <summary>
        /// Predict a new solution based on the previous ones
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        protected void Predict(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            var current = IntegrationStates[0];
            var previous = IntegrationStates[1];

            // Predict a solution
            switch (Order)
            {
                case 1:
                    // Divided difference approach
                    for (var i = 1; i <= current.Solution.Length; i++)
                    {
                        var dd0 = (current.Solution[i] - previous.Solution[i]) / previous.Delta;
                        Prediction[i] = current.Solution[i] + current.Delta * dd0;
                    }
                    break;

                case 2:
                    // Adams-Bashforth method (second order for variable timesteps)
                    var second = IntegrationStates[2];
                    var b = -current.Delta / (2.0 * previous.Delta);
                    var a = 1 - b;
                    for (var i = 1; i <= current.Solution.Length; i++)
                    {
                        var dd0 = (current.Solution[i] - previous.Solution[i]) / previous.Delta;
                        var dd1 = (previous.Solution[i] - second.Solution[i]) / second.Delta;
                        Prediction[i] = current.Solution[i] + (b * dd1 + a * dd0) * current.Delta;
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
        /// <param name="simulation">The time simulation</param>
        /// <returns></returns>
        protected double TruncateNodes(TimeSimulation simulation)
        {
            // Get the state
            var state = simulation.RealState;
            double tol, diff, tmp;
            var timetemp = Double.PositiveInfinity;
            var nodes = simulation.Nodes;
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
            return timetemp;
        }

        /// <summary>
        /// Compute the coefficients for Trapezoidal integration
        /// </summary>
        protected void ComputeCoefficients()
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
    }
}
