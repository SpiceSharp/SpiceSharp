using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="MutualInductance"/>
    /// </summary>
    public class TimeBehavior : TemperatureBehavior, ITimeBehavior
    {
        private readonly ElementSet<double> _elements;
        private double _conductance;
        private readonly InductorBehaviors.TimeBehavior _load1, _load2;
        private readonly int _br1, _br2;
        private readonly ITimeSimulationState _time;
        private readonly IBiasingSimulationState _biasing;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context"></param>
        public TimeBehavior(string name, MutualInductanceBindingContext context) : base(name, context)
        {
            _time = context.GetState<ITimeSimulationState>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _load1 = context.Inductor1Behaviors.GetValue<InductorBehaviors.TimeBehavior>();
            _br1 = _biasing.Map[_load1.Branch];
            _load2 = context.Inductor2Behaviors.GetValue<InductorBehaviors.TimeBehavior>();
            _br2 = _biasing.Map[_load2.Branch];

            // Register events for modifying the flux through the inductors
            _load1.UpdateFlux += UpdateFlux1;
            _load2.UpdateFlux += UpdateFlux2;

            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(_br1, _br2),
                new MatrixLocation(_br2, _br1));
        }

        /// <summary>
        /// Update the flux through the secondary inductor.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux2(object sender, UpdateFluxEventArgs args)
        {
            var state = args.State;
            args.Flux.Value += Factor * state.Solution[_br1];
        }

        /// <summary>
        /// Update the flux through the primary inductor.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux1(object sender, UpdateFluxEventArgs args)
        {
            var state = args.State;
            _conductance = args.Flux.GetContributions(Factor).Jacobian;
            args.Flux.Value += Factor * state.Solution[_br2];
        }

        /// <summary>
        /// Initialize states.
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
                return;

            // Load Y-matrix
            _elements.Add(-_conductance, -_conductance);
        }
    }
}
