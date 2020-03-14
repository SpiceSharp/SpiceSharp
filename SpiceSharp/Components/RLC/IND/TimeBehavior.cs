using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Transient behavior for an <see cref="Inductor" />.
    /// </summary>
    public class TimeBehavior : BiasingBehavior, ITimeBehavior
    {
        private readonly ElementSet<double> _elements;
        private readonly int _branchEq;
        private readonly IDerivative _flux;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Gets the flux of the inductor.
        /// </summary>
        [ParameterName("flux"), ParameterInfo("The flux through the inductor.")]
        public double Flux => _flux.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TimeBehavior(string name, IComponentBindingContext context) : base(name, context)
        {
            _branchEq = BiasingState.Map[Branch];
            _time = context.GetState<ITimeSimulationState>();
            _elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_branchEq, _branchEq)
            }, new[] { _branchEq });

            var method = context.GetState<IIntegrationMethod>();
            _flux = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Get the current through
            if (Parameters.InitialCondition.Given)
                _flux.Value = Parameters.InitialCondition * Inductance;
            else
                _flux.Value = BiasingState.Solution[_branchEq] * Inductance;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;

            // Initialize
            _flux.Value = Inductance * BiasingState.Solution[_branchEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                var args = new UpdateFluxEventArgs(Inductance, BiasingState.Solution[_branchEq], _flux, BiasingState);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            _flux.Integrate();
            var info = _flux.GetContributions(Inductance);
            _elements.Add(
                -info.Jacobian,
                info.Rhs
                );
        }
    }
}
