using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class Trapezoidal
    {
        /// <summary>
        /// An <see cref="IIntegrationMethod"/> that implements the trapezoidal method.
        /// </summary>
        /// <seealso cref="SpiceMethod.SpiceInstance" />
        protected partial class Instance : SpiceInstance
        {
            private const int _trapezoidalOrder = 2;
            private int _stateValues = 0;
            private readonly double _xmu;

            /// <summary>
            /// The integration coefficients.
            /// </summary>
            protected readonly double[] Coefficients = new double[_trapezoidalOrder];

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <param name="state">The biasing simulation state.</param>
            public Instance(Trapezoidal parameters, IBiasingSimulationState state)
                : base(parameters, state, _trapezoidalOrder)
            {
                _xmu = parameters.Xmu;
            }

            /// <inheritdoc/>
            public override void Initialize()
            {
                // Create all the states
                States.Set(i => new SpiceIntegrationState(0.0,
                    new DenseVector<double>(State.Solver.Size),
                    _stateValues));

                // Reset all integration coefficients
                for (int i = 0; i < Coefficients.Length; i++)
                    Coefficients[i] = 0.0;

                base.Initialize();

                // Add our own truncatable states
                if (Parameters.TruncateNodes)
                    TruncatableStates.Add(new NodeTruncation(this));
            }

            /// <inheritdoc/>
            public override IDerivative CreateDerivative(bool track = true)
            {
                var derivative = new DerivativeInstance(this, _stateValues + 1);
                _stateValues += 2;
                if (track)
                    TruncatableStates.Add(derivative);
                return derivative;
            }

            /// <inheritdoc/>
            public override IIntegral CreateIntegral(bool track = true)
            {
                var integral = new IntegralInstance(this, _stateValues + 1);
                _stateValues += 2;
                if (track)
                    TruncatableStates.Add(integral);
                return integral;
            }

            /// <inheritdoc/>
            protected override void ComputeCoefficients()
            {
                double delta = States.Value.Delta;

                // Integration constants
                switch (Order)
                {
                    case 1:
                        Coefficients[0] = 1.0 / delta;
                        Coefficients[1] = -1.0 / delta;
                        break;

                    case 2:
                        Coefficients[0] = 1.0 / delta / (1.0 - _xmu);
                        Coefficients[1] = _xmu / (1.0 - _xmu);
                        break;
                }

                // Store the derivative w.r.t. the current timestep
                Slope = Coefficients[0];
            }

            /// <inheritdoc/>
            protected override void Predict()
            {
                // Use the two previous solutions to predict a new one (the one we're about to test)
                var future = States.Value;
                var current = States.GetPreviousValue(1);
                var previous = States.GetPreviousValue(2);

                // Predict a solution
                switch (Order)
                {
                    case 1:
                        // Divided difference approach
                        for (int i = 1; i <= Prediction.Length; i++)
                        {
                            double dd0 = (current.Solution[i] - previous.Solution[i]) / current.Delta;
                            Prediction[i] = current.Solution[i] + future.Delta * dd0;
                        }
                        break;

                    case 2:
                        // Adams-Bashforth method (second order for variable timesteps)
                        var second = States.GetPreviousValue(3);
                        double b = -future.Delta / (2.0 * current.Delta);
                        double a = 1 - b;
                        for (int i = 1; i <= Prediction.Length; i++)
                        {
                            double dd0 = (current.Solution[i] - previous.Solution[i]) / current.Delta;
                            double dd1 = (previous.Solution[i] - second.Solution[i]) / previous.Delta;
                            Prediction[i] = current.Solution[i] + (b * dd1 + a * dd0) * future.Delta;
                        }
                        break;
                }
            }
        }
    }
}
