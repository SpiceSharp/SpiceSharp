using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;

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
        /// Gets the (primary, secondary) branch element.
        /// </summary>
        protected MatrixElement<Complex> Branch1Branch2Ptr { get; private set; }

        /// <summary>
        /// Gets the (secondary, primary) branch element.
        /// </summary>
        protected MatrixElement<Complex> Branch2Branch1Ptr { get; private set; }


        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; private set; }

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
            Bias1 = c.Inductor1Behaviors.Get<BiasingBehavior>();
            Bias2 = c.Inductor2Behaviors.Get<BiasingBehavior>();

            ComplexState = context.States.Get<ComplexSimulationState>();
            var solver = ComplexState.Solver;
            Branch1Branch2Ptr = solver.GetMatrixElement(Bias1.BranchEq, Bias2.BranchEq);
            Branch2Branch1Ptr = solver.GetMatrixElement(Bias2.BranchEq, Bias1.BranchEq);
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

            // Load Y-matrix
            Branch1Branch2Ptr.Value -= value;
            Branch2Branch1Ptr.Value -= value;
        }
    }
}
