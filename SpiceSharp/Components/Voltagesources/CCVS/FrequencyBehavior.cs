using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior, IBranchedBehavior<Complex>
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;
        private readonly IVariable<Complex> _control;

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <returns>
        /// The voltage.
        /// </returns>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns>
        /// The current.
        /// </returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => Branch.Value;

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns>
        /// The power dissipation.
        /// </returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        /// <value>
        /// The branch.
        /// </value>
        public new IVariable<Complex> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context"></param>
        public FrequencyBehavior(string name, ICurrentControlledBindingContext context) : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<Complex>>().Branch;
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            var pos = _complex.Map[_variables.Positive];
            var neg = _complex.Map[_variables.Negative];
            var cbr = _complex.Map[_control];
            var br = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, cbr));
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(1, -1, 1, -1, -Parameters.Coefficient);
        }
    }
}
