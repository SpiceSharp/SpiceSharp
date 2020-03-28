using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly int _posNode, _negNode, _brNode;
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// Get the voltage. 
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _complex.Solution[_posNode] - _complex.Solution[_negNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => _complex.Solution[_brNode] * Parameters.Coefficient;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower
        {
            get
            {
                var v = _complex.Solution[_posNode] - _complex.Solution[_negNode];
                var i = _complex.Solution[_brNode] * Parameters.Coefficient;
                return -v * Complex.Conjugate(i);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ICurrentControlledBindingContext context) : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            
            _posNode = _complex.Map[_complex.MapNode(context.Nodes[0])];
            _negNode = _complex.Map[_complex.MapNode(context.Nodes[1])];
            _brNode = _complex.Map[context.ControlBehaviors.GetValue<IBranchedBehavior<Complex>>().Branch];

            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_negNode, _brNode));
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        public void InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var value = Parameters.Coefficient;
            _elements.Add(value, -value);
        }
    }
}
