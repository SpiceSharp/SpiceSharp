using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    public partial class CurrentSource
    {
        /// <summary>
        /// Behavior of a currentsource in AC analysis
        /// </summary>
        /// <seealso cref="BiasingBehavior"/>
        /// <seealso cref="IFrequencyBehavior"/>
        protected class FrequencyBehavior : BiasingBehavior,
            IFrequencyBehavior
        {
            private readonly IComplexSimulationState _complex;
            private readonly OnePort<Complex> _variables;
            private readonly ElementSet<Complex> _elements;

            /// <summary>
            /// Get the complex voltage over the source.
            /// </summary>
            /// <value>
            /// The complex voltage.
            /// </value>
            [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
            public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

            /// <summary>
            /// Get the power dissipation.
            /// </summary>
            /// <value>
            /// The complex power dissipation.
            /// </value>
            [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
            public Complex ComplexPower
            {
                get
                {
                    var v = _variables.Positive.Value - _variables.Negative.Value;
                    return -v * Complex.Conjugate(Parameters.Phasor);
                }
            }

            /// <summary>
            /// Get the current.
            /// </summary>
            /// <value>
            /// The complex current.
            /// </value>
            [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
            public Complex ComplexCurrent => Parameters.Phasor;

            /// <summary>
            /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
            /// </summary>
            /// <param name="name">Name</param>
            /// <param name="context">The context.</param>
            public FrequencyBehavior(string name, ComponentBindingContext context)
                : base(name, context)
            {
                _complex = context.GetState<IComplexSimulationState>();
                _variables = new OnePort<Complex>(_complex, context);
                _elements = new ElementSet<Complex>(_complex.Solver, null, _variables.GetRhsIndices(_complex.Map));
            }

            void IFrequencyBehavior.InitializeParameters()
            {
            }

            void IFrequencyBehavior.Load()
            {
                // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
                // I solved it by inverting the current when loading the rhs vector
                var value = Parameters.Phasor;
                _elements.Add(-value, value);
            }
        }
    }
}
