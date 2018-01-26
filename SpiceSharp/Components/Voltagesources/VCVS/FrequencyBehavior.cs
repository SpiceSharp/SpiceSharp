using System;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System.Numerics;

namespace SpiceSharp.Components.VoltageControlledVoltagesourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, contPosNode, contNegNode, branchEq;
        protected MatrixElement PosIbrPtr { get; private set; }
        protected MatrixElement NegIbrPtr { get; private set; }
        protected MatrixElement IbrPosPtr { get; private set; }
        protected MatrixElement IbrNegPtr { get; private set; }
        protected MatrixElement IbrControlPosPtr { get; private set; }
        protected MatrixElement IbrControlNegPtr { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[posNode] - state.Solution[negNode],
                state.iSolution[posNode] - state.iSolution[negNode]);
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Complex current")]
        public Complex GetCurrent(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[branchEq],
                state.iSolution[branchEq]);
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = new Complex(
                state.Solution[posNode] - state.Solution[negNode],
                state.iSolution[posNode] - state.iSolution[negNode]);
            Complex i = new Complex(
                state.Solution[branchEq],
                state.iSolution[branchEq]);
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
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);
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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
            contPosNode = pins[2];
            contNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            branchEq = load.BranchEq;
            PosIbrPtr = matrix.GetElement(posNode, branchEq);
            NegIbrPtr = matrix.GetElement(negNode, branchEq);
            IbrPosPtr = matrix.GetElement(branchEq, posNode);
            IbrNegPtr = matrix.GetElement(branchEq, negNode);
            IbrControlPosPtr = matrix.GetElement(branchEq, contPosNode);
            IbrControlNegPtr = matrix.GetElement(branchEq, contNegNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosIbrPtr = null;
            NegIbrPtr = null;
            IbrPosPtr = null;
            IbrNegPtr = null;
            IbrControlPosPtr = null;
            IbrControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            PosIbrPtr.Add(1.0);
            IbrPosPtr.Add(1.0);
            NegIbrPtr.Sub(1.0);
            IbrNegPtr.Sub(1.0);
            IbrControlPosPtr.Sub(bp.Coefficient);
            IbrControlNegPtr.Add(bp.Coefficient);
        }
    }
}
