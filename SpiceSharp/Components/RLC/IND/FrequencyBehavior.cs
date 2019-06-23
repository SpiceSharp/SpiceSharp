using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Inductor"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private BiasingBehavior _base;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _branchEq;
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchBranchPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

            // Get behaviors
            _base = provider.GetBehavior<BiasingBehavior>();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			solver.ThrowIfNull(nameof(solver));

            // Get current equation
            _branchEq = _base.BranchEq;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(_posNode, _branchEq);
            NegBranchPtr = solver.GetMatrixElement(_negNode, _branchEq);
            BranchNegPtr = solver.GetMatrixElement(_branchEq, _negNode);
            BranchPosPtr = solver.GetMatrixElement(_branchEq, _posNode);
            BranchBranchPtr = solver.GetMatrixElement(_branchEq, _branchEq);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.ComplexState;
            var val = state.Laplace * _bp.Inductance.Value;

            PosBranchPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;
            BranchPosPtr.Value += 1.0;
            BranchBranchPtr.Value -= val;
        }
    }
}
