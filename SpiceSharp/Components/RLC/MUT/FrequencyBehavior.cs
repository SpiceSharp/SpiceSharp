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

        private int _br1, _br2;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, MutualInductanceBindingContext context) : base(name, context) 
        {
            ComplexState = context.GetState<IComplexSimulationState>();
            var bias = context.Inductor1Behaviors.GetValue<BiasingBehavior>();
            _br1 = ComplexState.Map[bias.Branch];
            bias = context.Inductor2Behaviors.GetValue<BiasingBehavior>();
            _br2 = ComplexState.Map[bias.Branch];

            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_br1, _br2),
                new MatrixLocation(_br2, _br1));
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
