using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="Inductor"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        
        /// <summary>
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Nodes
        /// </summary>
        int BranchEq;
        protected MatrixElement<double> BranchBranchPtr { get; private set; }
        protected VectorElement<double> BranchPtr { get; private set; }
        StateDerivative flux;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="propertyName">Property</param>
        /// <returns></returns>
        public override Func<RealState, double> CreateExport(string propertyName)
        {
            switch (propertyName)
            {
                case "flux": return (RealState state) => flux.Current;
                default: return null;
            }
        }

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

            // Clear all events
            if (UpdateFlux != null)
            {
                foreach (var inv in UpdateFlux.GetInvocationList())
                    UpdateFlux -= (EventHandler<UpdateFluxEventArgs>)inv;
            }
        }

        /// <summary>
        /// Gets matrix pointer
        /// </summary>
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get current equation
            BranchEq = load.BranchEq;
            BranchPtr = solver.GetRhsElement(BranchEq);

            // Get matrix pointers
            BranchBranchPtr = solver.GetMatrixElement(BranchEq, BranchEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            BranchBranchPtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            flux = states.CreateDerivative();
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Get the current through
            if (bp.InitialCondition.Given)
                flux.Current = bp.InitialCondition * bp.Inductance;
            else
                flux.Current = simulation.RealState.Solution[BranchEq] * bp.Inductance;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;

            // Initialize
            flux.Current = bp.Inductance * state.Solution[BranchEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                UpdateFluxEventArgs args = new UpdateFluxEventArgs(bp.Inductance, state.Solution[BranchEq], flux, state);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            flux.Integrate();
            BranchPtr.Value += flux.RhsCurrent();
            BranchBranchPtr.Value -= flux.Jacobian(bp.Inductance);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            flux.LocalTruncationError(ref timestep);
        }
    }
}
