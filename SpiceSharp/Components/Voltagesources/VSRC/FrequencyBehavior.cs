using System;
using System.Numerics;
using SpiceSharp.NewSparse;
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
        public Complex AC { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, branchEq;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected MatrixElement<Complex> BranchBranchPtr { get; private set; }
        protected VectorElement<Complex> BranchPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex Voltage => AC;
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
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            var ap = provider.GetParameterSet<FrequencyParameters>("entity");

            // Calculate AC vector
            double radians = ap.ACPhase * Math.PI / 180.0;
            AC = new Complex(ap.ACMagnitude * Math.Cos(radians), ap.ACMagnitude * Math.Sin(radians));

            // Get behaviors
            var load = provider.GetBehavior<LoadBehavior>("entity");
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get matrix elements
            PosBranchPtr = solver.GetMatrixElement(posNode, branchEq);
            BranchPosPtr = solver.GetMatrixElement(branchEq, posNode);
            NegBranchPtr = solver.GetMatrixElement(negNode, branchEq);
            BranchNegPtr = solver.GetMatrixElement(branchEq, negNode);

            // Get rhs elements
            BranchPtr = solver.GetRhsElement(branchEq);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosBranchPtr = null;
            BranchPosPtr = null;
            NegBranchPtr = null;
            BranchNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));
            var state = simulation.ComplexState;

            // Load Y-matrix
            PosBranchPtr.Value += 1.0;
            BranchPosPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;

            // Load Rhs-vector
            BranchPtr.Value += AC;
        }
    }
}
