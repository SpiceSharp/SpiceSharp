using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Inductor"/>
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
        int posNode, negNode, branchEq;
        protected MatrixElement PosIbrPtr { get; private set; }
        protected MatrixElement NegIbrPtr { get; private set; }
        protected MatrixElement IbrNegPtr { get; private set; }
        protected MatrixElement IbrPosPtr { get; private set; }
        protected MatrixElement IbrIbrPtr { get; private set; }

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
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get current equation
            branchEq = load.BranchEq;

            // Get matrix pointers
            PosIbrPtr = matrix.GetElement(posNode, branchEq);
            NegIbrPtr = matrix.GetElement(negNode, branchEq);
            IbrNegPtr = matrix.GetElement(branchEq, negNode);
            IbrPosPtr = matrix.GetElement(branchEq, posNode);
            IbrIbrPtr = matrix.GetElement(branchEq, branchEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            PosIbrPtr = null;
            NegIbrPtr = null;
            IbrPosPtr = null;
            IbrNegPtr = null;
            IbrIbrPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            Complex val = state.Laplace * bp.Inductance.Value;

            PosIbrPtr.Add(1.0);
            NegIbrPtr.Sub(1.0);
            IbrNegPtr.Sub(1.0);
            IbrPosPtr.Add(1.0);
            IbrIbrPtr.Sub(val);
        }
    }
}
