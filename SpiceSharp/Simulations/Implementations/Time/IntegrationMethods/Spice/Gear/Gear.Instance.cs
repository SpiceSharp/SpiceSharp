﻿using SpiceSharp.Algebra;

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
            protected readonly DenseVector<double> Coefficients = new DenseVector<double>(_gearOrder + 1);

            /// <summary>
            /// The prediction coefficients.
            /// </summary>
            protected readonly DenseVector<double> PredictionCoefficients = new DenseVector<double>(_gearOrder + 1);

            /// <summary>
            /// The solver used to calculate the coefficients.
            /// </summary>
            protected readonly DenseRealSolver Solver = new DenseRealSolver(_gearOrder + 1);

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance" /> class.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <param name="state">The biasing simulation state.</param>
            public Instance(Gear parameters, IBiasingSimulationState state)
                : base(parameters, state, _gearOrder)
            {
            }

            /// <summary>
            /// Initializes the integration method using the allocated biasing state.
            /// At this point, all entities should have received the chance to allocate and register integration states.
            /// </summary>
            public override void Initialize()
            {
                // Create all the states
                States.Set(i => new SpiceIntegrationState(0.0,
                    new DenseVector<double>(State.Solver.Size),
                    _stateValues));

                // Reset all integration coefficients
                for (var i = 0; i <= _gearOrder; i++)
                {
                    Coefficients[i] = 0.0;
                    PredictionCoefficients[i] = 0.0;
                }

                base.Initialize();

                // Add our own truncatable states
                if (Parameters.TruncateNodes)
                    TruncatableStates.Add(new NodeTruncation(this));
            }

            /// <summary>
            /// Creates a derivative.
            /// </summary>
            /// <param name="track">If set to <c>true</c>, the integration method will use this state to limit truncation errors.</param>
            /// <returns>
            /// The derivative.
            /// </returns>
            public override IDerivative CreateDerivative(bool track)
            {
                var derivative = new DerivativeInstance(this, _stateValues + 1);
                _stateValues += 2;
                if (track)
                    TruncatableStates.Add(derivative);
                return derivative;
            }

            /// <summary>
            /// Computes the integration coefficients.
            /// </summary>
            protected override void ComputeCoefficients()
            {
                var delta = States.Value.Delta;

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
                    arg += States.GetPreviousValue(i - 2).Delta;
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
                    arg += States.GetPreviousValue(i - 1).Delta;
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
            /// Predicts a solution for truncation.
            /// </summary>
            protected override void Predict()
            {
                // Use the previous solutions to predict a new one
                for (var i = 0; i <= Prediction.Length; i++)
                {
                    Prediction[i] = 0.0;
                    for (var k = 0; k <= Order; k++)
                    {
                        Prediction[i] += PredictionCoefficients[k + 1] * States.GetPreviousValue(k + 1).Solution[i];
                    }
                }
            }
        }
    }
}
