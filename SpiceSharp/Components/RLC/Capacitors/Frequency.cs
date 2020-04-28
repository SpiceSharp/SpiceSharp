using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.Capacitors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Capacitor"/>.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Temperature,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Gets the complex voltage.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex ComplexCurrent => ComplexVoltage * _complex.Laplace * Capacitance;

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        /// <value>
        /// The complex power.
        /// </value>
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
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, IComponentBindingContext context)
            : base(name, context)
        {
            context.Nodes.CheckNodes(2);
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            _elements = new ElementSet<Complex>(_complex.Solver, _variables.GetMatrixLocations(_complex.Map));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var val = _complex.Laplace * Capacitance;
            _elements.Add(val, -val, -val, val);
        }
    }
}