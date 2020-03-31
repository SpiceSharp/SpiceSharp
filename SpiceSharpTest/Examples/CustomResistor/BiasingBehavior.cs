using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.NonlinearResistorBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="NonlinearResistor"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private int _nodeA, _nodeB;
        private ElementSet<double> _elements;
        private BaseParameters _bp;
        private IBiasingSimulationState _state;
        private BiasingParameters _baseConfig;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            _bp = context.GetParameterSet<BaseParameters>();
            _state = context.GetState<IBiasingSimulationState>();
            _baseConfig = context.GetSimulationParameterSet<BiasingParameters>();

            // Find the nodes that the resistor is connected to
            _nodeA = _state.Map[_state.GetSharedVariable(context.Nodes[0])];
            _nodeB = _state.Map[_state.GetSharedVariable(context.Nodes[1])];

            // We need 4 matrix elements and 2 RHS vector elements
            _elements = new ElementSet<double>(_state.Solver, new[] {
                    new MatrixLocation(_nodeA, _nodeA),
                    new MatrixLocation(_nodeA, _nodeB),
                    new MatrixLocation(_nodeB, _nodeA),
                    new MatrixLocation(_nodeB, _nodeB)
                }, new[] {
                    _nodeA,
                    _nodeB
                });
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            // First get the current iteration voltage
            var v = _state.Solution[_nodeA] - _state.Solution[_nodeB];

            // Calculate the derivative w.r.t. one of the voltages
            var isNegative = v < 0;
            var c = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B);
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
