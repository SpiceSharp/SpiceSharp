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
        protected MatrixElement PosIbrptr { get; private set; }
        protected MatrixElement NegIbrptr { get; private set; }
        protected MatrixElement IbrPosptr { get; private set; }
        protected MatrixElement IbrNegptr { get; private set; }
        protected MatrixElement IbrIbrptr { get; private set; }

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

            PosIbrptr = matrix.GetElement(posNode, branchEq);
            IbrPosptr = matrix.GetElement(branchEq, posNode);
            NegIbrptr = matrix.GetElement(negNode, branchEq);
            IbrNegptr = matrix.GetElement(branchEq, negNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosIbrptr = null;
            IbrPosptr = null;
            NegIbrptr = null;
            IbrNegptr = null;
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
            PosIbrptr.Value.Real += 1.0;
            IbrPosptr.Value.Real += 1.0;
            NegIbrptr.Value.Real -= 1.0;
            IbrNegptr.Value.Real -= 1.0;
            cstate.Rhs[branchEq] += Ac.Real;
            cstate.iRhs[branchEq] += Ac.Imaginary;
        }
    }
}
