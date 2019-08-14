using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Transient behavior for an <see cref="Inductor" />.
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }
        
        /// <summary>
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Gets the (branch, branch) element.
        /// </summary>
        protected MatrixElement<double> BranchBranchPtr { get; private set; }

        /// <summary>
        /// Gets the branch RHS element.
        /// </summary>
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// The state tracking the flux.
        /// </summary>
        private StateDerivative _flux;

        /// <summary>
        /// Gets the flux of the inductor.
        /// </summary>
        [ParameterName("flux"), ParameterInfo("The flux through the inductor.")]
        public double Flux => _flux.Current;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name)
        {
        }

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
            BaseParameters = provider.GetParameterSet<BaseParameters>();

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
			solver.ThrowIfNull(nameof(solver));

            // Get vector elements
            BranchPtr = solver.GetRhsElement(BranchEq);

            // Get matrix elements
            BranchBranchPtr = solver.GetMatrixElement(BranchEq, BranchEq);
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
			method.ThrowIfNull(nameof(method));
            _flux = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            // Get the current through
            if (BaseParameters.InitialCondition.Given)
                _flux.Current = BaseParameters.InitialCondition * BaseParameters.Inductance;
            else
                _flux.Current = simulation.RealState.Solution[BranchEq] * BaseParameters.Inductance;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.RealState;

            // Initialize
            _flux.Current = BaseParameters.Inductance * state.Solution[BranchEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                var args = new UpdateFluxEventArgs(BaseParameters.Inductance, state.Solution[BranchEq], _flux, state);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            _flux.Integrate();
            BranchPtr.Value += _flux.RhsCurrent();
            BranchBranchPtr.Value -= _flux.Jacobian(BaseParameters.Inductance);
        }
    }
}
