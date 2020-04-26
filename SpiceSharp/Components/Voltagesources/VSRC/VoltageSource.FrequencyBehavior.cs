using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    public partial class VoltageSource
    {
        /// <summary>
        /// AC behavior for <see cref="VoltageSource"/>
        /// </summary>
        /// <seealso cref="BiasingBehavior"/>
        /// <seealso cref="IFrequencyBehavior"/>
        /// <seealso cref="IBranchedBehavior{T}"/>
        protected class FrequencyBehavior : BiasingBehavior, 
            IFrequencyBehavior, 
            IBranchedBehavior<Complex>
        {
            private readonly OnePort<Complex> _variables;
            private readonly ElementSet<Complex> _elements;
            private readonly IComplexSimulationState _complex;

            /// <summary>
            /// Gets the complex voltage applied by the source.
            /// </summary>
            /// <value>
            /// The complex voltage applied by the source.
            /// </value>
            [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
            public Complex ComplexVoltage => Parameters.Phasor;

            /// <summary>
            /// Gets the current through the source.
            /// </summary>
            /// <value>
            /// The complex current through the source.
            /// </value>
            [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
            public Complex ComplexCurrent => Branch.Value;

            /// <summary>
            /// Gets the comlex power dissipated by the source.
            /// </summary>
            /// <value>
            /// The complex power dissipated by the source.
            /// </value>
            [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
            public Complex ComplexPower => -Voltage * Complex.Conjugate(Branch.Value);

            /// <inheritdoc/>
            public new IVariable<Complex> Branch { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public FrequencyBehavior(string name, IComponentBindingContext context) : base(name, context)
            {
                // Connections
                _complex = context.GetState<IComplexSimulationState>();
                _variables = new OnePort<Complex>(_complex, context);
                Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

                var pos = _complex.Map[_variables.Positive];
                var neg = _complex.Map[_variables.Negative];
                var br = _complex.Map[Branch];

                _elements = new ElementSet<Complex>(_complex.Solver, new[] {
                        new MatrixLocation(pos, br),
                        new MatrixLocation(br, pos),
                        new MatrixLocation(neg, br),
                        new MatrixLocation(br, neg)
                    }, new[] { br });
            }

            void IFrequencyBehavior.InitializeParameters()
            {
            }

            void IFrequencyBehavior.Load()
            {
                _elements.Add(1, 1, -1, -1, Parameters.Phasor);
            }
        }
    }
}
