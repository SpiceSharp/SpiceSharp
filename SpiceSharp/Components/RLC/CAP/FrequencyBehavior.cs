using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Capacitor"/>.
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly int _posNode, _negNode;
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex ComplexVoltage => _complex.Solution[_posNode] - _complex.Solution[_negNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex ComplexCurrent
        {
            get
            {
                var conductance = _complex.Laplace * Capacitance;
                return (_complex.Solution[_posNode] - _complex.Solution[_negNode]) * conductance;
            }
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex ComplexPower
        {
            get
            {
                var conductance = _complex.Laplace * Capacitance;
                var voltage = _complex.Solution[_posNode] - _complex.Solution[_negNode];
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
            _posNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[0])];
            _negNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[1])];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
                );
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
