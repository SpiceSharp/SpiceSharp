using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="MutualInductance"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        InductorBehaviors.LoadBehavior load1, load2;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement Branch1Branch2Ptr { get; private set; }
        protected MatrixElement Branch2Branch1Ptr { get; private set; }

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
            bp = provider.GetParameterSet<BaseParameters>(0);
            var bp1 = provider.GetParameterSet<InductorBehaviors.BaseParameters>(1);
            var bp2 = provider.GetParameterSet<InductorBehaviors.BaseParameters>(2);

            // Get behaviors
            load1 = provider.GetBehavior<InductorBehaviors.LoadBehavior>(1);
            load2 = provider.GetBehavior<InductorBehaviors.LoadBehavior>(2);

            // Calculate coupling factor
            Factor = bp.Coupling * Math.Sqrt(bp1.Inductance * bp2.Inductance);
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            int INDbrEq1 = load1.BranchEq;
            int INDbrEq2 = load2.BranchEq;

            // Get matrix equations
            Branch1Branch2Ptr = matrix.GetElement(INDbrEq1, INDbrEq2);
            Branch2Branch1Ptr = matrix.GetElement(INDbrEq2, INDbrEq1);
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

            var state = simulation.State;
            Complex value = state.Laplace * Factor;
            Branch1Branch2Ptr.Sub(value);
            Branch2Branch1Ptr.Sub(value);
        }
    }
}
