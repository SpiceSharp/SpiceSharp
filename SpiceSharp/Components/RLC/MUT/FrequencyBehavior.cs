using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="MutualInductance"/>
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the <see cref="BiasingBehavior"/> of the primary inductor.
        /// </summary>
        protected BiasingBehavior Bias1 { get; private set; }

        /// <summary>
        /// Gets the <see cref="BiasingBehavior"/> of the secondary inductor.
        /// </summary>
        protected BiasingBehavior Bias2 { get; private set; }

        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
			base.Bind(context);
            var c = (MutualInductanceBindingContext)context;
            Bias1 = c.Inductor1Behaviors.GetValue<BiasingBehavior>();
            Bias2 = c.Inductor2Behaviors.GetValue<BiasingBehavior>();

            ComplexState = context.States.GetValue<IComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(Bias1.BranchEq, Bias2.BranchEq),
                new MatrixLocation(Bias2.BranchEq, Bias1.BranchEq));
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var value = ComplexState.Laplace * Factor;
            ComplexElements.Add(-value, -value);
        }
    }
}
