using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior,
        IParameterized<IndependentSourceFrequencyParameters>
    {
        private readonly IComplexSimulationState _complex;
        private readonly int _posNode, _negNode;
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        /// <value>
        /// The frequency parameters.
        /// </value>
        public IndependentSourceFrequencyParameters FrequencyParameters { get; }

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        IndependentSourceFrequencyParameters IParameterized<IndependentSourceFrequencyParameters>.Parameters => FrequencyParameters;

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _complex.Solution[_posNode] - _complex.Solution[_negNode];

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower
        {
            get
            {
                var v = _complex.Solution[_posNode] - _complex.Solution[_negNode];
                return -v * Complex.Conjugate(FrequencyParameters.Phasor);
            }
        }

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => FrequencyParameters.Phasor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context"></param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            FrequencyParameters = context.GetParameterSet<IndependentSourceFrequencyParameters>();
            _complex = context.GetState<IComplexSimulationState>();
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];
            _elements = new ElementSet<Complex>(_complex.Solver, null, new[] { _posNode, _negNode });
        }

        /// <summary>
        /// Initializes the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            var value = FrequencyParameters.Phasor;
            _elements.Add(-value, value);
        }
    }
}
