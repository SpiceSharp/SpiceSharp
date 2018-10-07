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

        // Constructor
        public LoadBehavior(string name) : base(name)
        {
        }

        // Get the base parameters
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
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
            var g = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B - 1.0) / _bp.A;

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
