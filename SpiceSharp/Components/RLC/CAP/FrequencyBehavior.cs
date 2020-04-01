using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Capacitor"/>.
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex ComplexCurrent => ComplexVoltage * _complex.Laplace * Capacitance;

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex ComplexPower
        {
            get
            {
                var conductance = _complex.Laplace * Capacitance;
                var voltage = ComplexVoltage;
                return voltage * Complex.Conjugate(voltage * conductance);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, IComponentBindingContext context) : base(name, context) 
        {
            context.Nodes.CheckNodes(2);
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            _elements = new ElementSet<Complex>(_complex.Solver, _variables.GetMatrixLocations(_complex.Map));
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
            var val = _complex.Laplace * Capacitance;
            _elements.Add(val, -val, -val, val);
        }
    }
}
