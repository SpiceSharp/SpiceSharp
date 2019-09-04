﻿using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;

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
        /// Gets the (primary, secondary) branch element.
        /// </summary>
        protected MatrixElement<double> Branch1Branch2 { get; private set; }

        /// <summary>
        /// Gets the (secondary, primary) branch element.
        /// </summary>
        protected MatrixElement<double> Branch2Branch1 { get; private set; }

        /// <summary>
        /// Gets the conductance.
        /// </summary>
        protected double Conductance { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected BiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
			base.Bind(context);
            var c = (MutualInductanceBindingContext)context;
            Load1 = c.Inductor1Behaviors.Get<InductorBehaviors.TransientBehavior>();
            Load2 = c.Inductor2Behaviors.Get<InductorBehaviors.TransientBehavior>();
            
            // Register events for modifying the flux through the inductors
            Load1.UpdateFlux += UpdateFlux1;
            Load2.UpdateFlux += UpdateFlux2;

            BiasingState = context.States.Get<BiasingSimulationState>();
            var solver = BiasingState.Solver;
            Branch1Branch2 = solver.GetMatrixElement(Load1.BranchEq, Load2.BranchEq);
            Branch2Branch1 = solver.GetMatrixElement(Load2.BranchEq, Load1.BranchEq);
        }

        /// <summary>
        /// Unsetup the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            // Remove events
            Load1.UpdateFlux -= UpdateFlux1;
            Load2.UpdateFlux -= UpdateFlux2;

            BiasingState = null;
            Branch1Branch2 = null;
            Branch2Branch1 = null;
        }

        /// <summary>
        /// Update the flux through the secondary inductor.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux2(object sender, UpdateFluxEventArgs args)
        {
            var state = args.State;
            args.Flux.Current += Factor * state.Solution[Load1.BranchEq];
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
            args.Flux.Current += Factor * state.Solution[Load2.BranchEq];
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
            Branch1Branch2.Value -= Conductance;
            Branch2Branch1.Value -= Conductance;
        }
    }
}
