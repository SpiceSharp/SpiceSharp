using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.VoltageControlledCurrentSources
{
    /// <summary>
    /// DC biasing behavior for a <see cref="VoltageControlledCurrentSource" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="VoltageControlledCurrentSources.Parameters"/>
    [BehaviorFor(typeof(VoltageControlledCurrentSource)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class BiasingBehavior : Behavior,
        IBiasingBehavior,
        IParameterized<Parameters>
    {
        private readonly ElementSet<double> _elements;
        private readonly IBiasingSimulationState _biasing;
        private readonly TwoPort<double> _variables;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double Current => (_variables.Left.Positive.Value - _variables.Left.Negative.Value) * Parameters.Transconductance;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public BiasingBehavior(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            _biasing = context.GetState<IBiasingSimulationState>();
            Parameters = context.GetParameterSet<Parameters>();
            _variables = new TwoPort<double>(_biasing, context);

            int pos = _biasing.Map[_variables.Right.Positive];
            int neg = _biasing.Map[_variables.Right.Negative];
            int contPos = _biasing.Map[_variables.Left.Positive];
            int contNeg = _biasing.Map[_variables.Left.Negative];
            _elements = new ElementSet<double>(_biasing.Solver, new[] {
                        new MatrixLocation(pos, contPos),
                        new MatrixLocation(pos, contNeg),
                        new MatrixLocation(neg, contPos),
                        new MatrixLocation(neg, contNeg)
                    });
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            double value = Parameters.Transconductance * Parameters.ParallelMultiplier;
            _elements.Add(value, -value, -value, value);
        }
    }
}