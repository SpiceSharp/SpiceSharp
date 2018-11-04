using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="Inductor"/>
    /// </summary>
    public partial class BaseBehavior : ITimeBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        
        /// <summary>
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<double> BranchBranchPtr { get; private set; }
        protected VectorElement<double> BranchPtr { get; private set; }
        private StateDerivative _flux;

        [ParameterName("flux"), ParameterInfo("The flux through the inductor.")]
        public double Flux => _flux.Current;

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public void Setup(Simulation simulation, SetupDataProvider provider)
        {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

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
        public void GetEquationPointers(Solver<double> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get current equation
            BranchPtr = solver.GetRhsElement(BranchEq);

            // Get matrix pointers
            BranchBranchPtr = solver.GetMatrixElement(BranchEq, BranchEq);
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

            _flux = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Get the current through
            if (_bp.InitialCondition.Given)
                _flux.Current = _bp.InitialCondition * _bp.Inductance;
            else
                _flux.Current = simulation.RealState.Solution[BranchEq] * _bp.Inductance;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;

            // Initialize
            _flux.Current = _bp.Inductance * state.Solution[BranchEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                var args = new UpdateFluxEventArgs(_bp.Inductance, state.Solution[BranchEq], _flux, state);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            _flux.Integrate();
            BranchPtr.Value += _flux.RhsCurrent();
            BranchBranchPtr.Value -= _flux.Jacobian(_bp.Inductance);
        }
    }
}
