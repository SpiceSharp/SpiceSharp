using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior, IBranchedBehavior<Complex>,
        IParameterized<IndependentSourceFrequencyParameters>
    {
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;

        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        /// <value>
        /// The frequency parameters.
        /// </value>
        public IndependentSourceFrequencyParameters FrequencyParameters { get; }
        IndependentSourceFrequencyParameters IParameterized<IndependentSourceFrequencyParameters>.Parameters => FrequencyParameters;

        /// <summary>
        /// Gets the complex voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => FrequencyParameters.Phasor;

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => Branch.Value;

        /// <summary>
        /// Gets the power through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -Voltage * Complex.Conjugate(Branch.Value);

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        public new IVariable<Complex> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, IComponentBindingContext context) : base(name, context)
        {
            FrequencyParameters = context.GetParameterSet<IndependentSourceFrequencyParameters>();

            // Connections
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            var pos = _complex.Map[_variables.Positive];
            var neg = _complex.Map[_variables.Negative];
            var br = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver, new[] {
                new MatrixLocation(pos, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, neg)
            }, new[] { br });
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
            _elements.Add(1, 1, -1, -1, FrequencyParameters.Phasor);
        }
    }
}
