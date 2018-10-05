using System;
using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="Inductor"/>
    /// </summary>
    public class TransientBehavior : BaseTransientBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load;
        
        /// <summary>
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _branchEq;
        protected MatrixElement<double> BranchBranchPtr { get; private set; }
        protected VectorElement<double> BranchPtr { get; private set; }
        private StateDerivative _flux;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Creates a getter for extracting data from the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="propertyName">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// A getter that returns the value of the specified parameter, or <c>null</c> if no parameter was found.
        /// </returns>
        public override Func<double> CreateGetter(Simulation simulation, string propertyName, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;
            if (comparer.Equals("flux", propertyName))
                return () => _flux.Current;
            return base.CreateGetter(simulation, propertyName, comparer);
        }

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

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();

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
            _branchEq = _load.BranchEq;
            BranchPtr = solver.GetRhsElement(_branchEq);

            // Get matrix pointers
            BranchBranchPtr = solver.GetMatrixElement(_branchEq, _branchEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            BranchBranchPtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public override void CreateStates(IntegrationMethod method)
        {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

            _flux = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Get the current through
            if (_bp.InitialCondition.Given)
                _flux.Current = _bp.InitialCondition * _bp.Inductance;
            else
                _flux.Current = simulation.RealState.Solution[_branchEq] * _bp.Inductance;
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
            _flux.Current = _bp.Inductance * state.Solution[_branchEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                var args = new UpdateFluxEventArgs(_bp.Inductance, state.Solution[_branchEq], _flux, state);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            _flux.Integrate();
            BranchPtr.Value += _flux.RhsCurrent();
            BranchBranchPtr.Value -= _flux.Jacobian(_bp.Inductance);
        }
    }
}
