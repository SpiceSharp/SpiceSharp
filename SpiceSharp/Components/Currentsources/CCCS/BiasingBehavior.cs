using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentControlledCurrentSource" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior,
        IParameterized<BaseParameters>
    {
        private readonly int _posNode, _negNode, _brNode;
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
        /// The branch variable controlling this source.
        /// </summary>
        protected Variable ControlBranch { get; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double Current => _biasing.Solution[_brNode] * Parameters.Coefficient;

        /// <summary>
        /// Gets the volage over the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double Voltage => _biasing.Solution[_posNode] - _biasing.Solution[_negNode];

        /// <summary>
        /// The power dissipation by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => (_biasing.Solution[_posNode] - _biasing.Solution[_negNode]) * _biasing.Solution[_brNode] * Parameters.Coefficient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ControlledBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);
            Parameters = context.GetParameterSet<BaseParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _posNode = _biasing.Map[context.Nodes[0]];
            _negNode = _biasing.Map[context.Nodes[1]];
            var load = context.ControlBehaviors.GetValue<IBranchedBehavior>();
            ControlBranch = load.Branch;
            _brNode = _biasing.Map[ControlBranch];
            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_negNode, _brNode));
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var value = Parameters.Coefficient.Value;
            _elements.Add(value, -value);
        }
    }
}
