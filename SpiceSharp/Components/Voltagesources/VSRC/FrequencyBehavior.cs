using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        protected CommonBehaviors.IndependentSourceFrequencyParameters FrequencyParameters { get; private set; }

        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Gets the complex voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public new Complex Voltage => FrequencyParameters.Phasor;

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => ComplexState.ThrowIfNotBound(this).Solution[_brNode];

        /// <summary>
        /// Gets the power through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var v = ComplexState.Solution[_posNode] - ComplexState.Solution[_negNode];
            var i = ComplexState.Solution[_brNode];
            return -v * Complex.Conjugate(i);
        }

        private int _posNode, _negNode, _brNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            FrequencyParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceFrequencyParameters>();

            // Connections
            ComplexState = context.GetState<IComplexSimulationState>();
            _posNode = ComplexState.Map[context.Nodes[0]];
            _negNode = ComplexState.Map[context.Nodes[1]];
            _brNode = ComplexState.Map[Branch];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver, new[] {
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_brNode, _posNode),
                new MatrixLocation(_negNode, _brNode),
                new MatrixLocation(_brNode, _negNode)
            }, new[] { _brNode });
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
            ComplexElements
                .ThrowIfNotBound(this)
                .Add(1, 1, -1, -1, FrequencyParameters.Phasor);
        }
    }
}
