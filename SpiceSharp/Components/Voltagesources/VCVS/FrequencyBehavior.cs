using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledVoltagesourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _contPosourceNode, _contNegateNode, _branchEq;
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected MatrixElement<Complex> BranchControlPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchControlNegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Complex current")]
        public Complex GetCurrent(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[_branchEq];
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = state.Solution[_posNode] - state.Solution[_negNode];
            Complex i = state.Solution[_branchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

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

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>("entity");
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
            _contPosourceNode = pins[2];
            _contNegateNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            _branchEq = _load.BranchEq;
            PosBranchPtr = solver.GetMatrixElement(_posNode, _branchEq);
            NegBranchPtr = solver.GetMatrixElement(_negNode, _branchEq);
            BranchPosPtr = solver.GetMatrixElement(_branchEq, _posNode);
            BranchNegPtr = solver.GetMatrixElement(_branchEq, _negNode);
            BranchControlPosPtr = solver.GetMatrixElement(_branchEq, _contPosourceNode);
            BranchControlNegPtr = solver.GetMatrixElement(_branchEq, _contNegateNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchControlPosPtr = null;
            BranchControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Load Y-matrix
            PosBranchPtr.Value += 1.0;
            BranchPosPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;
            BranchControlPosPtr.Value -= _bp.Coefficient.Value;
            BranchControlNegPtr.Value += _bp.Coefficient.Value;
        }
    }
}
