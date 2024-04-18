using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.CurrentControlledCurrentSources
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentControlledCurrentSource" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="CurrentControlledCurrentSources.Parameters"/>
    [BehaviorFor(typeof(CurrentControlledCurrentSource)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IParameterized<Parameters>
    {
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _control;
        private readonly IBiasingSimulationState _biasing;
        private readonly ElementSet<double> _elements;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double Current => _control.Value * Parameters.Coefficient;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The context for the behavior.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the simulation does not define an <see cref="IComplexSimulationState"/>.</exception>
        /// <exception cref="TypeNotFoundException">Thrown if the controlling entity does not have a behavior of type <see cref="IBranchedBehavior{T}"/>.</exception>
        public Biasing(ICurrentControlledBindingContext context) : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);
            Parameters = context.GetParameterSet<Parameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(_biasing, context);
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;

            int pos = _biasing.Map[_variables.Positive];
            int neg = _biasing.Map[_variables.Negative];
            int br = _biasing.Map[_control];
            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br));
        }

        void IBiasingBehavior.Load()
        {
            double value = Parameters.Coefficient * Parameters.ParallelMultiplier;
            _elements.Add(value, -value);
        }
    }
}