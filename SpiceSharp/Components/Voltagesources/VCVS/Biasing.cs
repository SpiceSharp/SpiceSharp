using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.VoltageControlledVoltageSources
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="VoltageControlledVoltageSources.Parameters"/>
    [BehaviorFor(typeof(VoltageControlledVoltageSource)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IBranchedBehavior<double>,
        IParameterized<Parameters>
    {
        private readonly TwoPort<double> _variables;
        private readonly IBiasingSimulationState _biasing;
        private readonly ElementSet<double> _elements;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Output current")]
        public double Current => Branch.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Output current")]
        public double Voltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            Parameters = context.GetParameterSet<Parameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _variables = new TwoPort<double>(_biasing, context);
            Branch = _biasing.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            int pos = _biasing.Map[_variables.Right.Positive];
            int neg = _biasing.Map[_variables.Right.Negative];
            int contPos = _biasing.Map[_variables.Left.Positive];
            int contNeg = _biasing.Map[_variables.Left.Negative];
            int br = _biasing.Map[Branch];

            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, contPos),
                new MatrixLocation(br, contNeg));
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            double val = Parameters.Coefficient;
            _elements.Add(1, -1, 1, -1, -val, val);
        }
    }
}
