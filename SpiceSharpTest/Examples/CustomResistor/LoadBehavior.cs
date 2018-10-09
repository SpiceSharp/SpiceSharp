using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.NonlinearResistorBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="NonlinearResistor"/>
    /// </summary>
    /// <seealso cref="BaseLoadBehavior" />
    /// <seealso cref="IConnectedBehavior" />
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        private int _nodeA, _nodeB;
        private MatrixElement<double> _aaPtr, _abPtr, _baPtr, _bbPtr;
        private VectorElement<double> _aPtr, _bPtr;
        private BaseParameters _bp;
        private BaseConfiguration _baseConfig;

        // Constructor
        public LoadBehavior(string name) : base(name)
        {
        }

        // Get the base parameters
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            _baseConfig = simulation.Configurations.Get<BaseConfiguration>();
            _bp = provider.GetParameterSet<BaseParameters>();
        }

        // Remove all references cached during setup
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
        }

        // Connect the behavior
        public void Connect(params int[] pins)
        {
            // Our nonlinear resistor will pass us the indices of the rows
            // for the KCL laws of node A and B.
            _nodeA = pins[0]; // Node A
            _nodeB = pins[1]; // Node B
        }

        // Allocate Y-matrix and RHS-vector elements
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            // We need 4 matrix elements here
            _aaPtr = solver.GetMatrixElement(_nodeA, _nodeA);
            _abPtr = solver.GetMatrixElement(_nodeA, _nodeB);
            _baPtr = solver.GetMatrixElement(_nodeB, _nodeA);
            _bbPtr = solver.GetMatrixElement(_nodeB, _nodeB);

            // We also need 2 RHS vector elements
            _aPtr = solver.GetRhsElement(_nodeA);
            _bPtr = solver.GetRhsElement(_nodeB);
        }

        // Load the behavior in the Y-matrix and RHS-vector
        public override void Load(BaseSimulation simulation)
        {
            var state = simulation.RealState;

            // First get the current iteration voltage
            var v = state.Solution[_nodeA] - state.Solution[_nodeB];

            // Calculate the derivative w.r.t. one of the voltages
            var isNegative = v < 0;
            var c = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B);
            double g;

            // Isolate special cases for the derivative
            if (_bp.B.Equals(1.0))
            {
                // i = v/a
                g = 1.0 / _bp.A;
            }
            else
            {
                // If v=0 the derivative is either 0 or infinity (avoid 0^(negative number))
                if (v.Equals(0.0))
                    g = _bp.B < 1.0 / _bp.A ? double.PositiveInfinity : 0.0;
                else
                    g = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B - 1.0) / _bp.A;
            }

            // At v=0
            g += _baseConfig.Gmin;

            // If the voltage was reversed, reverse the current
            if (isNegative)
                c = -c;
            
            // Load the RHS vector
            c = c - g * v;
            _aPtr.Value += c;
            _bPtr.Value -= c;

            // Load the Y-matrix
            _aaPtr.Value += g;
            _abPtr.Value -= g;
            _baPtr.Value -= g;
            _bbPtr.Value += g;
        }
    }
}
