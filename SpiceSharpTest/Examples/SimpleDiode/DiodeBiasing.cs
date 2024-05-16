using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.DiodeBehaviors
{
    /// <summary>
    /// Biasing behavior for a diode.
    /// </summary>
    public class DiodeBiasing : DiodeTemperature, IBiasingBehavior
    {
        private readonly IVariable<double> _variableA, _variableB;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Creates a new diode biasing behavior.
        /// </summary>
        /// <param name="context"></param>
        public DiodeBiasing(IComponentBindingContext context)
            : base(context)
        {
            var biasingState = context.GetState<IBiasingSimulationState>();

            // Get the variables that our diode is connected to
            _variableA = biasingState.GetSharedVariable(context.Nodes[0]);
            _variableB = biasingState.GetSharedVariable(context.Nodes[1]);

            // Get the rows in the solver that represent the KCL equations
            int rowA = biasingState.Map[_variableA];
            int rowB = biasingState.Map[_variableB];
            _elements = new ElementSet<double>(biasingState.Solver,
                [
                    // The Y-matrix elements
                    new(rowA, rowA),
                    new(rowA, rowB),
                    new(rowB, rowA),
                    new(rowB, rowB)
                ],
                [
                    // The right-hand side vector elements
                    rowA,
                    rowB
                ]);

        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        public void Load()
        {
            // Let us calculate the derivatives and the current
            double voltage = _variableA.Value - _variableB.Value;
            double current = Parameters.Iss * (Math.Exp(voltage / Vte) - 1.0);
            double derivative = current / Vte;

            // Load the Y-matrix and RHS vector
            double rhs = current - voltage * derivative;
            _elements.Add(
                // Y-matrix contributions
                derivative, -derivative,
                -derivative, derivative,
                // RHS vector contributions
                -rhs, rhs);
        }
    }
}
