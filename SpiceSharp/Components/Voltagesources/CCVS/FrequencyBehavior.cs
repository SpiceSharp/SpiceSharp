using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly int _posNode, _negNode, _cbrNode, _brNode;

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <returns>
        /// The voltage.
        /// </returns>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _complex.Solution[_posNode] - _complex.Solution[_negNode];

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns>
        /// The current.
        /// </returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => _complex.Solution[_brNode];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns>
        /// The power dissipation.
        /// </returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower
        {
            get
            {
                var v = _complex.Solution[_posNode] - _complex.Solution[_negNode];
                var i = _complex.Solution[_brNode];
                return -v * Complex.Conjugate(i);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context"></param>
        public FrequencyBehavior(string name, ControlledBindingContext context) : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];
            _cbrNode = _complex.Map[ControlBranch];
            _brNode = _complex.Map[Branch];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_negNode, _brNode),
                new MatrixLocation(_brNode, _posNode),
                new MatrixLocation(_brNode, _negNode),
                new MatrixLocation(_brNode, _cbrNode));
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(1, -1, 1, -1, -Parameters.Coefficient);
        }
    }
}
