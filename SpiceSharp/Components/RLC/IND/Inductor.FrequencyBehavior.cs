using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    public partial class Inductor
    {
        /// <summary>
        /// Frequency behavior for <see cref="Inductor"/>
        /// </summary>
        /// <seealso cref="BiasingBehavior"/>
        /// <seealso cref="IFrequencyBehavior"/>
        /// <seealso cref="IBranchedBehavior{T}"/>
        protected class FrequencyBehavior : BiasingBehavior, 
            IFrequencyBehavior, 
            IBranchedBehavior<Complex>
        {
            private readonly IComplexSimulationState _complex;
            private readonly ElementSet<Complex> _elements;
            private readonly OnePort<Complex> _variables;

            /// <inheritdoc/>
            public new IVariable<Complex> Branch { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public FrequencyBehavior(string name, IComponentBindingContext context) : base(name, context)
            {
                _complex = context.GetState<IComplexSimulationState>();
                _variables = new OnePort<Complex>(_complex, context);
                Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

                var pos = _complex.Map[_variables.Positive];
                var neg = _complex.Map[_variables.Negative];
                var br = _complex.Map[Branch];

                _elements = new ElementSet<Complex>(_complex.Solver,
                    new MatrixLocation(pos, br),
                    new MatrixLocation(neg, br),
                    new MatrixLocation(br, neg),
                    new MatrixLocation(br, pos),
                    new MatrixLocation(br, br));
            }

            void IFrequencyBehavior.InitializeParameters()
            {
            }

            void IFrequencyBehavior.Load()
            {
                var val = _complex.Laplace * Inductance;
                _elements.Add(1, -1, -1, 1, -val);
            }
        }
    }
}