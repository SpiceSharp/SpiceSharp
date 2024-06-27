using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;

namespace SpiceSharp.Components.VoltageSources
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSource"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(VoltageSource)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class FrequencyBehavior : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => Parameters.Phasor;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("i_c"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => Branch.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -Voltage * Complex.Conjugate(Branch.Value);

        /// <inheritdoc/>
        public new IVariable<Complex> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyBehavior(IComponentBindingContext context)
            : base(context)
        {
            // Connections
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            int pos = _complex.Map[_variables.Positive];
            int neg = _complex.Map[_variables.Negative];
            int br = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver, [
                        new MatrixLocation(pos, br),
                        new MatrixLocation(br, pos),
                        new MatrixLocation(neg, br),
                        new MatrixLocation(br, neg)
                    ], [br]);
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            Parameters.UpdatePhasor();
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(1, 1, -1, -1, Parameters.Phasor);
        }
    }
}