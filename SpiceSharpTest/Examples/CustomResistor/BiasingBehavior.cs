using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.NonlinearResistorBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="NonlinearResistor"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private int _nodeA, _nodeB;
        private RealOnePortElementSet _matrixElements;
        private RealVectorElementSet _vectorElements;
        private BaseParameters _bp;
        private BiasingSimulationState _state;
        private BiasingConfiguration _baseConfig;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public BiasingBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            // Cache some objects that we will use often
            var c = (ComponentBindingContext)context;
            _bp = context.Behaviors.Parameters.GetValue<BaseParameters>();
            _state = c.States.GetValue<BiasingSimulationState>();
            _baseConfig = c.Configurations.GetValue<BiasingConfiguration>();

            // Find the nodes that the resistor is connected to
            var cbc = (ComponentBindingContext)context;
                _nodeA = cbc.Pins[0];
                _nodeB = cbc.Pins[1];

            // We need 4 matrix elements here
            var solver = c.States.GetValue<BiasingSimulationState>().Solver;
            _matrixElements = new RealOnePortElementSet(solver, _nodeA, _nodeB);

            // We also need 2 RHS vector elements
            _vectorElements = new RealVectorElementSet(solver, _nodeA, _nodeB);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _bp = null;
            _state = null;
            _baseConfig = null;
            _matrixElements?.Destroy();
            _matrixElements = null;
            _vectorElements?.Destroy();
            _vectorElements = null;
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
            _vectorElements.Add(c, -c);

            // Load the Y-matrix
            _matrixElements.AddOnePort(g);
        }

        /// <summary>
        /// Check for convergence.
        /// </summary>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
