using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using System;
using System.Numerics;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        BaseParameters bp;
        VoltagesourceBehaviors.LoadBehavior vsrcload;

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[posourceNode] - state.Solution[negateNode];
        }
        [PropertyName("i"), PropertyInfo("Complex current")]
        public Complex GetCurrent(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[contBranch] * bp.Coefficient.Value;
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = state.Solution[posourceNode] - state.Solution[negateNode];
            Complex i = state.Solution[contBranch] * bp.Coefficient.Value;
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        int posourceNode, negateNode, contBranch;
        protected MatrixElement PosControlBranchPtr { get; private set; }
        protected MatrixElement NegControlBranchPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Get behaviors
            vsrcload = provider.GetBehavior<VoltagesourceBehaviors.LoadBehavior>(1);
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
            posourceNode = pins[0];
            negateNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            contBranch = vsrcload.BranchEq;
            PosControlBranchPtr = matrix.GetElement(posourceNode, contBranch);
            NegControlBranchPtr = matrix.GetElement(negateNode, contBranch);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosControlBranchPtr = null;
            NegControlBranchPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            PosControlBranchPtr.Add(bp.Coefficient);
            NegControlBranchPtr.Sub(bp.Coefficient);
        }
    }
}
