using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Resistor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior
    {
        private readonly int _posNode, _negNode;
        private readonly ElementSet<double> _elements;
        private readonly IBiasingSimulationState _biasing;

        /// <summary>
        /// Gets the voltage across the resistor.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => _biasing.Solution[_posNode] - _biasing.Solution[_negNode];

        /// <summary>
        /// Gets the current through the resistor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Current")]
        public double Current => (_biasing.Solution[_posNode] - _biasing.Solution[_negNode]) * Conductance;

        /// <summary>
        /// Gets the power dissipated by the resistor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double Power
        {
            get
            {
                var v = _biasing.Solution[_posNode] - _biasing.Solution[_negNode];
                return v * v * Conductance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, IComponentBindingContext context) : base(name, context)
        {
            context.Nodes.CheckNodes(2);
            _biasing = context.GetState<IBiasingSimulationState>();
            _posNode = _biasing.Map[_biasing.MapNode(context.Nodes[0])];
            _negNode = _biasing.Map[_biasing.MapNode(context.Nodes[1])];
            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode));
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            _elements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
