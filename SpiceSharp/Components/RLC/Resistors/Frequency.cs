using System.Numerics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Small-signal behavior for <see cref="Resistor"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent => ComplexVoltage * Conductance;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
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
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, IComponentBindingContext context) : base(name, context) 
        {
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(state, context);
            _elements = new ElementSet<Complex>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            _elements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
