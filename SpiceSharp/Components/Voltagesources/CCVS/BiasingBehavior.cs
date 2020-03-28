using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior<double>,
        IParameterized<BaseParameters>
    {
        private readonly OnePort<double> _variables;
        private readonly IBiasingSimulationState _biasing;
        private readonly ElementSet<double> _elements;
        private readonly IVariable<double> _control;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Output current")]
        public double Current => Branch.Value;

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Output voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        /// <value>
        /// The branch.
        /// </value>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ICurrentControlledBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            Parameters = context.GetParameterSet<BaseParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(_biasing, context);
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            Branch = _biasing.Create(Name.Combine("branch"), Units.Ampere);

            var pos = _biasing.Map[_variables.Positive];
            var neg = _biasing.Map[_variables.Negative];
            var cbr = _biasing.Map[_control];
            var br = _biasing.Map[Branch];

            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, cbr));
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            _elements.Add(1, -1, 1, -1, -Parameters.Coefficient);
        }
    }
}
