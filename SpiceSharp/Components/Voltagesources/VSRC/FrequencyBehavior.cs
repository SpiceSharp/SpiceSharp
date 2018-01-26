using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltagesourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// AC excitation vector
        /// </summary>
        public Complex Ac { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int posNode, negNode, branchEq;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement PosIbrPtr { get; private set; }
        protected MatrixElement NegIbrPtr { get; private set; }
        protected MatrixElement IbrPosPtr { get; private set; }
        protected MatrixElement IbrNegPtr { get; private set; }
        protected MatrixElement IbrIbrPtr { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex Voltage => Ac;
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
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            var ap = provider.GetParameterSet<FrequencyParameters>(0);

            // Calculate AC vector
            double radians = ap.AcPhase * Math.PI / 180.0;
            Ac = new Complex(ap.AcMagnitude * Math.Cos(radians), ap.AcMagnitude * Math.Sin(radians));

            // Get behaviors
            var load = provider.GetBehavior<LoadBehavior>(0);
            branchEq = load.BranchEq;
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

            PosIbrPtr = matrix.GetElement(posNode, branchEq);
            IbrPosPtr = matrix.GetElement(branchEq, posNode);
            NegIbrPtr = matrix.GetElement(negNode, branchEq);
            IbrNegPtr = matrix.GetElement(branchEq, negNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosIbrPtr = null;
            IbrPosPtr = null;
            NegIbrPtr = null;
            IbrNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var cstate = sim.State;
            PosIbrPtr.Value.Real += 1.0;
            IbrPosPtr.Value.Real += 1.0;
            NegIbrPtr.Value.Real -= 1.0;
            IbrNegPtr.Value.Real -= 1.0;
            cstate.Rhs[branchEq] += Ac.Real;
            cstate.iRhs[branchEq] += Ac.Imaginary;
        }
    }
}
