using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;
using LoadBehavior = SpiceSharp.Components.InductorBehaviors.LoadBehavior;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="MutualInductance"/>
    /// </summary>
    public class TransientBehavior : BaseTransientBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load1, _load2;
        private InductorBehaviors.TransientBehavior _tran1, _tran2;

        /// <summary>
        /// The factor
        /// </summary>
        public double Factor { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _branchEq1, _branchEq2;
        protected MatrixElement<double> Branch1Branch2 { get; private set; }
        protected MatrixElement<double> Branch2Branch1 { get; private set; }

        /// <summary>
        /// Conductance
        /// </summary>
        protected double Cond { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            var bp1 = provider.GetParameterSet<InductorBehaviors.BaseParameters>("inductor1");
            var bp2 = provider.GetParameterSet<InductorBehaviors.BaseParameters>("inductor2");

            // Get behaviors
            _load1 = provider.GetBehavior<LoadBehavior>("inductor1");
            _load2 = provider.GetBehavior<LoadBehavior>("inductor2");
            _tran1 = provider.GetBehavior<InductorBehaviors.TransientBehavior>("inductor1");
            _tran2 = provider.GetBehavior<InductorBehaviors.TransientBehavior>("inductor2");

            // Calculate coupling factor
            Factor = _bp.Coupling * Math.Sqrt(bp1.Inductance * bp2.Inductance);

            // Register events for modifying the flux through the inductors
            _tran1.UpdateFlux += UpdateFlux1;
            _tran2.UpdateFlux += UpdateFlux2;
        }

        /// <summary>
        /// Update the flux through inductor 2
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux2(object sender, UpdateFluxEventArgs args)
        {
            var state = args.State;
            args.Flux.Current += Factor * state.Solution[_load1.BranchEq];
        }

        /// <summary>
        /// Update the flux through inductor 1
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux1(object sender, UpdateFluxEventArgs args)
        {
            var state = args.State;
            Cond = args.Flux.Jacobian(Factor);
            args.Flux.Current += Factor * state.Solution[_load2.BranchEq];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            _branchEq1 = _load1.BranchEq;
            _branchEq2 = _load2.BranchEq;

            // Get matrix pointers
            Branch1Branch2 = solver.GetMatrixElement(_branchEq1, _branchEq2);
            Branch2Branch1 = solver.GetMatrixElement(_branchEq2, _branchEq1);
        }

        /// <summary>
        /// Unsetup behavior
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            Branch1Branch2 = null;
            Branch2Branch1 = null;

            // Remove events
            _tran1.UpdateFlux -= UpdateFlux1;
            _tran2.UpdateFlux -= UpdateFlux2;
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
            Branch1Branch2.Value -= Cond;
            Branch2Branch1.Value -= Cond;
        }
    }
}
