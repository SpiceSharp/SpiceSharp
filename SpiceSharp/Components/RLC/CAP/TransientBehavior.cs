using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Capacitor"/>
    /// </summary>
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("i"), PropertyInfo("Device current")]
        public double Current => QCap.Derivative;
        [PropertyName("p"), PropertyInfo("Instantaneous device power")]
        public double GetPower(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return QCap.Derivative * (state.Solution[_posNode] - state.Solution[_negNode]);
        }
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[_posNode] - state.Solution[_negNode];
        }

        /// <summary>
        /// Nodes and states
        /// </summary>
        private int _posNode, _negNode;
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }
        protected MatrixElement<double> PosNegPtr { get; private set; }
        protected MatrixElement<double> NegPosPtr { get; private set; }
        protected VectorElement<double> PosPtr { get; private set; }
        protected VectorElement<double> NegPtr { get; private set; }
        protected StateDerivative QCap { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            QCap = states.CreateDerivative();
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Get matrix elements
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
            NegPosPtr = solver.GetMatrixElement(_negNode, _posNode);
            PosNegPtr = solver.GetMatrixElement(_posNode, _negNode);

            // Get rhs elements
            PosPtr = solver.GetRhsElement(_posNode);
            NegPtr = solver.GetRhsElement(_negNode);
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        /// <param name="simulation"></param>
        public override void GetDcState(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Calculate the state for DC
            var sol = simulation.RealState.Solution;
            if (_bp.InitialCondition.Given)
                QCap.Current = _bp.InitialCondition;
            else
                QCap.Current = _bp.Capacitance * (sol[_posNode] - sol[_negNode]);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosPosPtr = null;
            NegNegPtr = null;
            NegPosPtr = null;
            PosNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double vcap = state.Solution[_posNode] - state.Solution[_negNode];

            // Integrate
            QCap.Current = _bp.Capacitance * vcap;
            QCap.Integrate();
            double geq = QCap.Jacobian(_bp.Capacitance);
            double ceq = QCap.RhsCurrent();

            // Load matrix
            PosPosPtr.Value += geq;
            NegNegPtr.Value += geq;
            PosNegPtr.Value -= geq;
            NegPosPtr.Value -= geq;

            // Load Rhs vector
            PosPtr.Value -= ceq;
            NegPtr.Value += ceq;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            QCap.LocalTruncationError(ref timestep);
        }
    }
}
