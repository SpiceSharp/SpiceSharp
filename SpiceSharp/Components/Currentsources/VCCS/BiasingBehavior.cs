using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="VoltageControlledCurrentSource" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior,
        IParameterized<BaseParameters>
    {
        private readonly ElementSet<double> _elements;
        private readonly IBiasingSimulationState _biasing;
        private readonly TwoPort<double> _variables;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double Current => (_variables.Left.Positive.Value - _variables.Left.Negative.Value) * Parameters.Coefficient;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, IComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            _biasing = context.GetState<IBiasingSimulationState>();
            Parameters = context.GetParameterSet<BaseParameters>();
            _variables = new TwoPort<double>(_biasing, context);

            var pos = _biasing.Map[_variables.Right.Positive];
            var neg = _biasing.Map[_variables.Right.Negative];
            var contPos = _biasing.Map[_variables.Left.Positive];
            var contNeg = _biasing.Map[_variables.Left.Negative];
            _elements = new ElementSet<double>(_biasing.Solver, new[] {
                new MatrixLocation(pos, contPos),
                new MatrixLocation(pos, contNeg),
                new MatrixLocation(neg, contPos),
                new MatrixLocation(neg, contNeg)
            });
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var value = Parameters.Coefficient;
            _elements.Add(value, -value, -value, value);
        }
    }
}
