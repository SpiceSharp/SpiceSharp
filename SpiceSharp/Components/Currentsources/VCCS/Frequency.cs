using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;

namespace SpiceSharp.Components.VoltageControlledCurrentSources
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledCurrentSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="IFrequencyBehavior"/>
    [BehaviorFor(typeof(VoltageControlledCurrentSource)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class FrequencyBehavior : BiasingBehavior,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly TwoPort<Complex> _variables;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => (_variables.Left.Positive.Value - _variables.Left.Negative.Value) * Parameters.Transconductance;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyBehavior(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new TwoPort<Complex>(_complex, context);

            int pos = _complex.Map[_variables.Right.Positive];
            int neg = _complex.Map[_variables.Right.Negative];
            int contPos = _complex.Map[_variables.Left.Positive];
            int contNeg = _complex.Map[_variables.Left.Negative];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(pos, contPos),
                new MatrixLocation(pos, contNeg),
                new MatrixLocation(neg, contPos),
                new MatrixLocation(neg, contNeg));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            double value = Parameters.Transconductance * Parameters.ParallelMultiplier;
            _elements.Add(value, -value, -value, value);
        }
    }
}