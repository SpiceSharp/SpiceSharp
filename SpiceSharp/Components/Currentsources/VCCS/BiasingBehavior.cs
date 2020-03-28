using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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
        private readonly int _posNode, _negNode, _contPosNode, _contNegNode;

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
        public double Voltage => _biasing.Solution[_posNode] - _biasing.Solution[_negNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double Current => (_biasing.Solution[_contPosNode] - _biasing.Solution[_contNegNode]) * Parameters.Coefficient;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power
        {
            get
            {
                var v = _biasing.Solution[_posNode] - _biasing.Solution[_negNode];
                var i = (_biasing.Solution[_contPosNode] - _biasing.Solution[_contNegNode]) * Parameters.Coefficient;
                return -v * i;
            }
        }

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
            _posNode = _biasing.Map[_biasing.MapNode(context.Nodes[0])];
            _negNode = _biasing.Map[_biasing.MapNode(context.Nodes[1])];
            _contPosNode = _biasing.Map[_biasing.MapNode(context.Nodes[2])];
            _contNegNode = _biasing.Map[_biasing.MapNode(context.Nodes[3])];
            _elements = new ElementSet<double>(_biasing.Solver, new[] {
                new MatrixLocation(_posNode, _contPosNode),
                new MatrixLocation(_posNode, _contNegNode),
                new MatrixLocation(_negNode, _contPosNode),
                new MatrixLocation(_negNode, _contNegNode)
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
