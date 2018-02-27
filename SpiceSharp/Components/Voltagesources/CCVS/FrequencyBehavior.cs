using System;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CurrentControlledVoltagesourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        VoltagesourceBehaviors.LoadBehavior vsrcload;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, branchEq, contBranchEq;
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected MatrixElement<Complex> BranchControlBranchPtr { get; private set; }

        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Complex current")]
        public Complex GetCurrent(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[branchEq];
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = state.Solution[posNode] - state.Solution[negNode];
            Complex i = state.Solution[branchEq];
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
            bp = provider.GetParameterSet<BaseParameters>("entity");

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>("entity");
            vsrcload = provider.GetBehavior<VoltagesourceBehaviors.LoadBehavior>("control");
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra nodes
            contBranchEq = vsrcload.BranchEq;
            branchEq = load.BranchEq;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(posNode, branchEq);
            NegBranchPtr = solver.GetMatrixElement(negNode, branchEq);
            BranchPosPtr = solver.GetMatrixElement(branchEq, posNode);
            BranchNegPtr = solver.GetMatrixElement(branchEq, negNode);
            BranchControlBranchPtr = solver.GetMatrixElement(branchEq, contBranchEq);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchControlBranchPtr = null;
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
            BranchControlBranchPtr.Value -= bp.Coefficient.Value;
        }
    }
}
