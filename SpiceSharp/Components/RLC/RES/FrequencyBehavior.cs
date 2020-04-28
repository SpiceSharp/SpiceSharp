using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Small-signal behavior for <see cref="Resistor"/>
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class FrequencyBehavior : BiasingBehavior,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Gets the (complex) voltage across the resistor.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v_c"), ParameterInfo("Complex voltage across the capacitor.")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the (complex) current through the resistor.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        [ParameterName("i_c"), ParameterInfo("Complex current through the capacitor.")]
        public Complex ComplexCurrent => ComplexVoltage * Conductance;

        /// <summary>
        /// Gets the (complex) power dissipated by the resistor.
        /// </summary>
        /// <value>
        /// The complex power.
        /// </value>
        [ParameterName("p_c"), ParameterInfo("Power")]
        public Complex ComplexPower
        {
            get
            {
                var voltage = ComplexVoltage;
                return voltage * Complex.Conjugate(voltage) * Conductance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
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
