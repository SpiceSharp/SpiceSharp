using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// Transient behavior for an <see cref="Inductor" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(Inductor)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    [GeneratedParameters]
    public partial class Time : Biasing,
        ITimeBehavior
    {
        private readonly ElementSet<double> _elements;
        private readonly IDerivative _flux;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Occurs when flux can be updated.
        /// </summary>
        /// <remarks>
        /// This event is used by <see cref="MutualInductance"/> to couple
        /// inductors.
        /// </remarks>
        public event EventHandler<UpdateFluxEventArgs> UpdateFlux;

        /// <summary>
        /// Gets the flux of the inductor.
        /// </summary>
        /// <value>
        /// The flux of the inductor.
        /// </value>
        [ParameterName("flux"), ParameterInfo("The flux through the inductor.")]
        public double Flux => _flux.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(IComponentBindingContext context) 
            : base(context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            int br = state.Map[Branch];
            _time = context.GetState<ITimeSimulationState>();
            _elements = new ElementSet<double>(state.Solver, [
                new MatrixLocation(br, br)
            ], [br]);

            var method = context.GetState<IIntegrationMethod>();
            _flux = method.CreateDerivative();
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            // Get the current through
            if (_time.UseIc && Parameters.InitialCondition.Given)
                _flux.Value = Parameters.InitialCondition * Inductance;
            else
                _flux.Value = Branch.Value * Inductance;
        }

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;

            // Initialize
            _flux.Value = Inductance * Branch.Value;

            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                var args = new UpdateFluxEventArgs(Inductance, Branch.Value, _flux);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            _flux.Derive();
            var info = _flux.GetContributions(Inductance);
            _elements.Add(
                -info.Jacobian,
                info.Rhs
                );
        }
    }
}