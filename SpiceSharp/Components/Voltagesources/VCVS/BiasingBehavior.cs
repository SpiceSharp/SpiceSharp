using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior,
        IParameterized<BaseParameters>
    {
        private readonly int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;
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
        public double Current => _biasing.Solution[_branchEq];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Output current")]
        public double Voltage => _biasing.Solution[_posNode] - _biasing.Solution[_negNode];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => _biasing.Solution[_branchEq] * (_biasing.Solution[_posNode] - _biasing.Solution[_negNode]);

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        public Variable Branch { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            Parameters = context.GetParameterSet<BaseParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _posNode = _biasing.Map[context.Nodes[0]];
            _negNode = _biasing.Map[context.Nodes[1]];
            _contPosNode = _biasing.Map[context.Nodes[2]];
            _contNegNode = _biasing.Map[context.Nodes[3]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
            _branchEq = _biasing.Map[Branch];
            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode));
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
