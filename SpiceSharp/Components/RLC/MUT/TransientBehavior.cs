using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="MutualInductance"/>
    /// </summary>
    public class TransientBehavior : TemperatureBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the transient behavior of the primary inductor.
        /// </summary>
        protected InductorBehaviors.TransientBehavior Load1 { get; private set; }

        /// <summary>
        /// Gets the transient behavior of secondary inductor.
        /// </summary>
        protected InductorBehaviors.TransientBehavior Load2 { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the equivalent conductance.
        /// </summary>
        protected double Conductance { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _br1, _br2;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context"></param>
        public TransientBehavior(string name, MutualInductanceBindingContext context) : base(name, context)
        {
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            Load1 = context.Inductor1Behaviors.GetValue<InductorBehaviors.TransientBehavior>();
            _br1 = BiasingState.Map[Load1.Branch];
            Load2 = context.Inductor2Behaviors.GetValue<InductorBehaviors.TransientBehavior>();
            _br2 = BiasingState.Map[Load2.Branch];

            // Register events for modifying the flux through the inductors
            Load1.UpdateFlux += UpdateFlux1;
            Load2.UpdateFlux += UpdateFlux2;

            Elements = new ElementSet<double>(BiasingState.Solver,
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
            args.Flux.Current += Factor * state.Solution[_br1];
        }

        /// <summary>
        /// Update the flux through the primary inductor.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux1(object sender, UpdateFluxEventArgs args)
        {
            var state = args.State;
            Conductance = args.Flux.Jacobian(Factor);
            args.Flux.Current += Factor * state.Solution[_br2];
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
        void ITimeBehavior.Load()
        {
            // Load Y-matrix
            Elements.Add(-Conductance, -Conductance);
        }
    }
}
