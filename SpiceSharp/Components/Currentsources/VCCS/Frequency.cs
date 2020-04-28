using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.VoltageControlledCurrentSources
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledCurrentSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class FrequencyBehavior : BiasingBehavior,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly TwoPort<Complex> _variables;

        /// <summary>
        /// Get the complex voltage.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <summary>
        /// Get the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => (_variables.Left.Positive.Value - _variables.Left.Negative.Value) * Parameters.Transconductance;

        /// <summary>
        /// Get the complex power dissipation.
        /// </summary>
        /// <value>
        /// The complex power dissipation.
        /// </value>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context)
            : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new TwoPort<Complex>(_complex, context);

            var pos = _complex.Map[_variables.Right.Positive];
            var neg = _complex.Map[_variables.Right.Negative];
            var contPos = _complex.Map[_variables.Left.Positive];
            var contNeg = _complex.Map[_variables.Left.Negative];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(pos, contPos),
                new MatrixLocation(pos, contNeg),
                new MatrixLocation(neg, contPos),
                new MatrixLocation(neg, contNeg));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var value = Parameters.Transconductance;
            _elements.Add(value, -value, -value, value);
        }
    }
}