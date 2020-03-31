using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior<double>,
        IParameterized<BaseParameters>
    {
        private readonly TwoPort<double> _variables;
        private readonly IBiasingSimulationState _biasing;
        private readonly ElementSet<double> _elements;

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
        [ParameterName("i"), ParameterName("i_r"), ParameterInfo("Output current")]
        public double Current => Branch.Value;

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Output current")]
        public double Voltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, IComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            Parameters = context.GetParameterSet<BaseParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _variables = new TwoPort<double>(_biasing, context);
            Branch = _biasing.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            var pos = _biasing.Map[_variables.Right.Positive];
            var neg = _biasing.Map[_variables.Right.Negative];
            var contPos = _biasing.Map[_variables.Left.Positive];
            var contNeg = _biasing.Map[_variables.Left.Negative];
            var br = _biasing.Map[Branch];

            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, contPos),
                new MatrixLocation(br, contNeg));
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var val = Parameters.Coefficient;
            _elements.Add(1, -1, 1, -1, -val, val);
        }
    }
}
