using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    public partial class Gear
    {
        /// <summary>
        /// An <see cref="IIntegrationMethod"/> that implements the Gear integration method.
        /// </summary>
        /// <seealso cref="SpiceMethod.SpiceInstance" />
        /// <seealso cref="IIntegrationMethod" />
        protected partial class Instance : SpiceInstance, IIntegrationMethod
        {
            private const int _gearOrder = 2;
            private int _stateValues = 0;

            /// <summary>
            /// The integration coefficients.
            /// </summary>
            protected readonly DenseVector<double> Coefficients = new(_gearOrder + 1);

            /// <summary>
            /// The prediction coefficients.
            /// </summary>
            protected readonly DenseVector<double> PredictionCoefficients = new(_gearOrder + 1);

            /// <summary>
            /// The solver used to calculate the coefficients.
            /// </summary>
            protected readonly DenseRealSolver Solver = new(_gearOrder + 1);

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance" /> class.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <param name="state">The biasing simulation state.</param>
            public Instance(Gear parameters, IBiasingSimulationState state)
                : base(parameters, state, _gearOrder)
            {
            }

            /// <inheritdoc/>
            public override void Initialize()
            {
                // Create all the states
                States.Set(i => new SpiceIntegrationState(0.0,
                    new DenseVector<double>(State.Solver.Size),
                    _stateValues));

                // Reset all integration coefficients
                for (int i = 0; i <= _gearOrder; i++)
                {
                    Coefficients[i] = 0.0;
                    PredictionCoefficients[i] = 0.0;
                }

                base.Initialize();

                // Add our own truncatable states
                if (Parameters.TruncateNodes)
                    TruncatableStates.Add(new NodeTruncation(this));
            }

            /// <inheritdoc/>
            public override IDerivative CreateDerivative(bool track)
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

                // Set up the matrix
                int n = Order + 1;
                for (int i = 1; i <= n; i++)
                    Solver[i] = 0.0;
                Solver[2] = -1 / delta;
                for (int i = 1; i <= n; i++)
                    Solver[1, i] = 1.0;
                for (int i = 2; i <= n; i++)
                    Solver[i, 1] = 0.0;

                double arg = 0.0, arg1;
                for (int i = 2; i <= n; i++)
                {
                    arg += States.GetPreviousValue(i - 2).Delta;
                    arg1 = 1.0;
                    for (int j = 2; j <= n; j++)
                    {
                        arg1 *= arg / delta;
                        Solver[j, i] = arg1;
                    }
                }
                Solver.Factor(n);
                Solver.ForwardSubstitute(Coefficients, n);
                Solver.BackwardSubstitute(Coefficients, n);

                // Predictor calculations
                for (int i = 2; i <= n; i++)
                    Solver[i] = 0.0;
                Solver[1] = 1.0;
                for (int i = 1; i <= n; i++)
                    Solver[1, i] = 1.0;
                arg = 0.0;
                for (int i = 1; i <= n; i++)
                {
                    arg += States.GetPreviousValue(i - 1).Delta;
                    arg1 = 1.0;
                    for (int j = 2; j <= n; j++)
                    {
                        arg1 *= arg / delta;
                        Solver[j, i] = arg1;
                    }
                }
                Solver.Factor(n);
                Solver.ForwardSubstitute(PredictionCoefficients, n);
                Solver.BackwardSubstitute(PredictionCoefficients, n);

                // Store the derivative w.r.t. the current timestep
                Slope = Coefficients[1];
            }

            /// <inheritdoc/>
            protected override void Predict()
            {
                // Use the previous solutions to predict a new one
                for (int i = 0; i <= Prediction.Length; i++)
                {
                    Prediction[i] = 0.0;
                    for (int k = 0; k <= Order; k++)
                    {
                        Prediction[i] += PredictionCoefficients[k + 1] * States.GetPreviousValue(k + 1).Solution[i];
                    }
                }
            }
        }
    }
}
