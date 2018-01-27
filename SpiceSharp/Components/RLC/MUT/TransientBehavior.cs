using System;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Components.MutualInductance"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        InductorBehaviors.LoadBehavior load1, load2;
        InductorBehaviors.TransientBehavior tran1, tran2;

        /// <summary>
        /// The factor
        /// </summary>
        public double Factor { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int BranchEq1, BranchEq2;
        protected MatrixElement Br1Br2 { get; private set; }
        protected MatrixElement Br2Br1 { get; private set; }

        /// <summary>
        /// Y-matrix contribution
        /// </summary>
        protected double Geq { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

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
            var bp1 = provider.GetParameterSet<Components.InductorBehaviors.BaseParameters>(1);
            var bp2 = provider.GetParameterSet<Components.InductorBehaviors.BaseParameters>(2);

            // Get behaviors
            load1 = provider.GetBehavior<InductorBehaviors.LoadBehavior>(1);
            load2 = provider.GetBehavior<InductorBehaviors.LoadBehavior>(2);
            tran1 = provider.GetBehavior<InductorBehaviors.TransientBehavior>(1);
            tran2 = provider.GetBehavior<InductorBehaviors.TransientBehavior>(2);

            // Calculate coupling factor
            Factor = bp.Coupling * Math.Sqrt(bp1.Inductance * bp2.Inductance);

            // Register events for modifying the flux through the inductors
            tran1.UpdateFlux += UpdateFlux1;
            tran2.UpdateFlux += UpdateFlux2;
        }

        /// <summary>
        /// Update the flux through inductor 2
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        void UpdateFlux2(object sender, InductorBehaviors.UpdateFluxEventArgs args)
        {
            var state = args.State;
            args.Flux.Value += Factor * state.Solution[load1.BranchEq];
        }

        /// <summary>
        /// Update the flux through inductor 1
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        void UpdateFlux1(object sender, InductorBehaviors.UpdateFluxEventArgs args)
        {
            var state = args.State;
            Geq = args.Flux.Jacobian(Factor);
            args.Flux.Value += Factor * state.Solution[load2.BranchEq];
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
            BranchEq1 = load1.BranchEq;
            BranchEq2 = load2.BranchEq;

            // Get matrix pointers
            Br1Br2 = matrix.GetElement(BranchEq1, BranchEq2);
            Br2Br1 = matrix.GetElement(BranchEq2, BranchEq1);
        }

        /// <summary>
        /// Unsetup behavior
        /// </summary>
        public override void Unsetup()
        {
            Br1Br2 = null;
            Br2Br1 = null;

            // Remove events
            tran1.UpdateFlux -= UpdateFlux1;
            tran2.UpdateFlux -= UpdateFlux2;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Load Y-matrix
            Br1Br2.Sub(Geq);
            Br2Br1.Sub(Geq);
        }
    }
}
