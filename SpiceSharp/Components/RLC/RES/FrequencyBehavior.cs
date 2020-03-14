using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Resistor"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly int _posNode, _negNode;
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// Gets the (complex) voltage across the resistor.
        /// </summary>
        [ParameterName("v_c"), ParameterInfo("Complex voltage across the capacitor.")]
        public Complex ComplexVoltage => _complex.Solution[_posNode] - _complex.Solution[_negNode];

        /// <summary>
        /// Gets the (complex) current through the resistor.
        /// </summary>
        [ParameterName("i_c"), ParameterInfo("Complex current through the capacitor.")]
        public Complex ComplexCurrent
        {
            get
            {
                var voltage = _complex.Solution[_posNode] - _complex.Solution[_negNode];
                return voltage * Conductance;
            }
        }

        /// <summary>
        /// Gets the (complex) power dissipated by the resistor.
        /// </summary>
        [ParameterName("p_c"), ParameterInfo("Power")]
        public Complex ComplexPower
        {
            get
            {
                var voltage = _complex.Solution[_posNode] - _complex.Solution[_negNode];
                return voltage * Complex.Conjugate(voltage) * Conductance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, IComponentBindingContext context) : base(name, context) 
        {
            _complex = context.GetState<IComplexSimulationState>();
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
                );
        }

        /// <summary>
        /// Initialize the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
