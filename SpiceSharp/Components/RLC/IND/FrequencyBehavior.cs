using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Algebra;
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
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchBranchPtr { get; private set; }

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

            // Get current equation
            branchEq = load.BranchEq;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(posNode, branchEq);
            NegBranchPtr = solver.GetMatrixElement(negNode, branchEq);
            BranchNegPtr = solver.GetMatrixElement(branchEq, negNode);
            BranchPosPtr = solver.GetMatrixElement(branchEq, posNode);
            BranchBranchPtr = solver.GetMatrixElement(branchEq, branchEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchBranchPtr = null;
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
            Complex val = state.Laplace * bp.Inductance.Value;

            PosBranchPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;
            BranchPosPtr.Value += 1.0;
            BranchBranchPtr.Value -= val;
        }
    }
}
