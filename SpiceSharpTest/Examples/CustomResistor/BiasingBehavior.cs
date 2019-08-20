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
        private MatrixElement<double> _aaPtr, _abPtr, _baPtr, _bbPtr;
        private VectorElement<double> _aPtr, _bPtr;
        private BaseParameters _bp;
        private BaseSimulationState _state;
        private BaseConfiguration _baseConfig;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public BiasingBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Cache some objects that we will use often
            _bp = context.GetParameterSet<BaseParameters>();
            _state = ((BaseSimulation)simulation).RealState;
            _baseConfig = simulation.Configurations.Get<BaseConfiguration>();

            // Find the nodes that the resistor is connected to
            if (context is ComponentBindingContext cbc)
            {
                _nodeA = cbc.Pins[0];
                _nodeB = cbc.Pins[1];
            }

            // We need 4 matrix elements here
            var solver = _state.Solver;
            _aaPtr = solver.GetMatrixElement(_nodeA, _nodeA);
            _abPtr = solver.GetMatrixElement(_nodeA, _nodeB);
            _baPtr = solver.GetMatrixElement(_nodeB, _nodeA);
            _bbPtr = solver.GetMatrixElement(_nodeB, _nodeB);

            // We also need 2 RHS vector elements
            _aPtr = solver.GetRhsElement(_nodeA);
            _bPtr = solver.GetRhsElement(_nodeB);
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
            _aaPtr = null;
            _abPtr = null;
            _baPtr = null;
            _bbPtr = null;
            _aPtr = null;
            _bPtr = null;
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
            _aPtr.Value += c;
            _bPtr.Value -= c;

            // Load the Y-matrix
            _aaPtr.Value += g;
            _abPtr.Value -= g;
            _baPtr.Value -= g;
            _bbPtr.Value += g;
        }

        /// <summary>
        /// Check for convergence.
        /// </summary>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
