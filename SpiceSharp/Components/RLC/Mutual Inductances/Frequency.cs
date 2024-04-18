using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;

namespace SpiceSharp.Components.MutualInductances
{
    /// <summary>
    /// Small-signal behavior for <see cref="MutualInductance"/>.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IFrequencyBehavior"/>
    [BehaviorFor(typeof(MutualInductance)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    public class Frequency : Temperature,
        IFrequencyBehavior
    {
        private readonly IVariable<Complex> _branch1, _branch2;
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(BindingContext context) 
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _branch1 = context.Inductor1Behaviors.GetValue<IBranchedBehavior<Complex>>().Branch;
            _branch2 = context.Inductor2Behaviors.GetValue<IBranchedBehavior<Complex>>().Branch;

            int br1 = _complex.Map[_branch1];
            int br2 = _complex.Map[_branch2];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(br1, br2),
                new MatrixLocation(br2, br1));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var value = _complex.Laplace * Factor;
            _elements.Add(-value, -value);
        }
    }
}