using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _complex.Solution[_posNode] - _complex.Solution[_negNode];

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => _complex.Solution[_branchEq];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower
        {
            get
            {
                var v = _complex.Solution[_posNode] - _complex.Solution[_negNode];
                var i = _complex.Solution[_branchEq];
                return -v * Complex.Conjugate(i);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];
            _contPosNode = _complex.Map[context.Nodes[2]];
            _contNegNode = _complex.Map[context.Nodes[3]];
            _branchEq = _complex.Map[Branch];
            _elements = new ElementSet<Complex>(_complex.Solver, new[] {
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode)
            });
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var value = Parameters.Coefficient.Value;
            _elements.Add(1, 1, -1, -1, -value, value);
        }
    }
}
