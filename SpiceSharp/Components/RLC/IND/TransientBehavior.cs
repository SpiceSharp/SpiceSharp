using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Transient behavior for an <see cref="Inductor" />.
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Gets the transient matrix elements.
        /// </summary>
        /// <value>
        /// The transient matrix elements.
        /// </value>
        protected ElementSet<double> TransientElements { get; private set; }

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
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            TransientElements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(BranchEq, BranchEq)
            }, new[] { BranchEq });

            var method = context.States.GetValue<TimeSimulationState>().Method;
            _flux = method.CreateDerivative();
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            // Clear all events
            if (UpdateFlux != null)
            {
                foreach (var inv in UpdateFlux.GetInvocationList())
                    UpdateFlux -= (EventHandler<UpdateFluxEventArgs>)inv;
            }

            TransientElements?.Destroy();
            TransientElements = null;
            _flux = null;
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Get the current through
            if (BaseParameters.InitialCondition.Given)
                _flux.Current = BaseParameters.InitialCondition * BaseParameters.Inductance;
            else
                _flux.Current = BiasingState.Solution[BranchEq] * BaseParameters.Inductance;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        void ITimeBehavior.Load()
        {
            // Initialize
            _flux.ThrowIfNotBound(this).Current = BaseParameters.Inductance * BiasingState.Solution[BranchEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                var args = new UpdateFluxEventArgs(BaseParameters.Inductance, BiasingState.Solution[BranchEq], _flux, BiasingState);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            _flux.Integrate();
            TransientElements.Add(
                -_flux.Jacobian(BaseParameters.Inductance),
                _flux.RhsCurrent()
                );
        }
    }
}
