using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Small-signal behavior for <see cref="Resistor"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    [BehaviorFor(typeof(Resistor)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent => ComplexVoltage * Conductance;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("The complex power")]
        public Complex ComplexPower
        {
            get
            {
                var voltage = ComplexVoltage;
                return voltage * Complex.Conjugate(voltage) * Conductance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(IComponentBindingContext context) : base(context)
        {
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(state, context);
            _elements = new ElementSet<Complex>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
