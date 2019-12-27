using SpiceSharp.Behaviors;
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
        private readonly int _br1, _br2;
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, MutualInductanceBindingContext context) : base(name, context) 
        {
            _complex = context.GetState<IComplexSimulationState>();
            var bias = context.Inductor1Behaviors.GetValue<IBranchedBehavior>();
            _br1 = _complex.Map[bias.Branch];
            bias = context.Inductor2Behaviors.GetValue<IBranchedBehavior>();
            _br2 = _complex.Map[bias.Branch];

            _elements = new ElementSet<Complex>(_complex.Solver,
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
            var value = _complex.Laplace * Factor;
            _elements.Add(-value, -value);
        }
    }
}
