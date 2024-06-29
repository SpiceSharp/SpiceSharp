using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.NonlinearResistorBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="NonlinearResistor"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private readonly IVariable<double> _nodeA, _nodeB;
        private readonly ElementSet<double> _elements;
        private readonly BaseParameters _bp;
        private readonly BiasingParameters _baseConfig;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The component binding context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Get our resistor parameters (we want our A and B parameter values)
            _bp = context.GetParameterSet<BaseParameters>();

            // Get the simulation parameters (we want to use Gmin)
            _baseConfig = context.GetSimulationParameterSet<BiasingParameters>();

            // Request the node variables
            var state = context.GetState<IBiasingSimulationState>();
            _nodeA = state.GetSharedVariable(context.Nodes[0]);
            _nodeB = state.GetSharedVariable(context.Nodes[1]);

            // We need 4 matrix elements and 2 RHS vector elements
            int indexA = state.Map[_nodeA];
            int indexB = state.Map[_nodeB];
            _elements = new ElementSet<double>(state.Solver, [
                    new MatrixLocation(indexA, indexA),
                    new MatrixLocation(indexA, indexB),
                    new MatrixLocation(indexB, indexA),
                    new MatrixLocation(indexB, indexB)
                ], [indexA, indexB]);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            // First get the current iteration voltage
            double v = _nodeA.Value - _nodeB.Value;

            // Calculate the derivative w.r.t. one of the voltages
            bool isNegative = v < 0;
            double c = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B);
            double g;

            // If v=0 the derivative is either 0 or infinity (avoid 0^(negative number) = not a number)
            if (v.Equals(0.0))
                g = _bp.B < 1.0 / _bp.A ? double.PositiveInfinity : 0.0;
            else
                g = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B - 1.0) / _bp.A;

            // In order to avoid having a singular matrix, we want to have at least a very small value here.
            g = Math.Max(g, _baseConfig.Gmin);

            // If the voltage was reversed, reverse the current back
            if (isNegative)
                c = -c;

            // Load the RHS vector
            c -= g * v;
            _elements.Add(
                // Y-matrix
                g, -g, -g, g,
                // RHS-vector
                c, -c);
        }
    }
}
