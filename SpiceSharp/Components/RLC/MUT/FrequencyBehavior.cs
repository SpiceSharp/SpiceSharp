using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using LoadBehavior = SpiceSharp.Components.InductorBehaviors.LoadBehavior;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="MutualInductance"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.BaseFrequencyBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load1, _load2;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement<Complex> Branch1Branch2Ptr { get; private set; }
        protected MatrixElement<Complex> Branch2Branch1Ptr { get; private set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double Factor { get; protected set; }

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
            var bp1 = provider.GetParameterSet<InductorBehaviors.BaseParameters>("inductor1");
            var bp2 = provider.GetParameterSet<InductorBehaviors.BaseParameters>("inductor2");

            // Get behaviors
            _load1 = provider.GetBehavior<LoadBehavior>("inductor1");
            _load2 = provider.GetBehavior<LoadBehavior>("inductor2");

            // Calculate coupling factor
            Factor = _bp.Coupling * Math.Sqrt(bp1.Inductance * bp2.Inductance);
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            int inDbrEq1 = _load1.BranchEq;
            int inDbrEq2 = _load2.BranchEq;

            // Get matrix equations
            Branch1Branch2Ptr = solver.GetMatrixElement(inDbrEq1, inDbrEq2);
            Branch2Branch1Ptr = solver.GetMatrixElement(inDbrEq2, inDbrEq1);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            Branch1Branch2Ptr = null;
            Branch2Branch1Ptr = null;
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
            Complex value = state.Laplace * Factor;

            // Load Y-matrix
            Branch1Branch2Ptr.Value -= value;
            Branch2Branch1Ptr.Value -= value;
        }
    }
}
