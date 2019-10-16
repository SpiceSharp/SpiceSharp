using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[_posNode] - ComplexState.Solution[_negNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => (ComplexState.Solution[_contPosNode] - ComplexState.Solution[_contNegNode]) * BaseParameters.Coefficient.Value;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var v = ComplexState.Solution[_posNode] - ComplexState.Solution[_negNode];
            var i = (ComplexState.Solution[_contPosNode] - ComplexState.Solution[_contNegNode]) * BaseParameters.Coefficient.Value;
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _posNode, _negNode, _contPosNode, _contNegNode;

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            var c = (ComponentBindingContext)context;
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            _posNode = ComplexState.Map[c.Nodes[0]];
            _negNode = ComplexState.Map[c.Nodes[1]];
            _contPosNode = ComplexState.Map[c.Nodes[2]];
            _contNegNode = ComplexState.Map[c.Nodes[3]];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_posNode, _contPosNode),
                new MatrixLocation(_posNode, _contNegNode),
                new MatrixLocation(_negNode, _contPosNode),
                new MatrixLocation(_negNode, _contNegNode));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexElements?.Destroy();
            ComplexElements = null;
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
            var value = BaseParameters.Coefficient.Value;
            ComplexElements.Add(value, -value, -value, value);
        }
    }
}
