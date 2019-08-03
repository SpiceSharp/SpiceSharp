using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.IntegrationMethods;
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
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
			base.Setup(simulation, provider);
			provider.ThrowIfNull(nameof(provider));
            
            // Get behaviors
            Load1 = provider.GetBehavior<InductorBehaviors.TransientBehavior>("inductor1");
            Load2 = provider.GetBehavior<InductorBehaviors.TransientBehavior>("inductor2");
            
            // Register events for modifying the flux through the inductors
            Load1.UpdateFlux += UpdateFlux1;
            Load2.UpdateFlux += UpdateFlux2;
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
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public void CreateStates(IntegrationMethod method)
        {
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
        }

        /// <summary>
        /// Gets the matrix pointers.
        /// </summary>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(Solver<double> solver)
        {
			solver.ThrowIfNull(nameof(solver));
            
            // Get matrix pointers
            Branch1Branch2 = solver.GetMatrixElement(Load1.BranchEq, Load2.BranchEq);
            Branch2Branch1 = solver.GetMatrixElement(Load2.BranchEq, Load1.BranchEq);
        }

        /// <summary>
        /// Unsetup the behavior.
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            base.Unsetup(simulation);

            // Remove events
            Load1.UpdateFlux -= UpdateFlux1;
            Load2.UpdateFlux -= UpdateFlux2;
        }

        /// <summary>
        /// Transient behavior.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            // Load Y-matrix
            Branch1Branch2.Value -= Conductance;
            Branch2Branch1.Value -= Conductance;
        }
    }
}
