using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="CurrentControlledVoltageSource"/>
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
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <returns>
        /// The voltage.
        /// </returns>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public new Complex GetVoltage()
        {
            var state = ComplexState.ThrowIfNotBound(this);
            return state.Solution[_posNode] - state.Solution[_negNode];
        }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns>
        /// The current.
        /// </returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public new Complex GetCurrent()
        {
            return ComplexState.ThrowIfNotBound(this).Solution[_brNode];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns>
        /// The power dissipation.
        /// </returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public new Complex GetPower()
        {
            var state = ComplexState.ThrowIfNotBound(this);
            var v = state.Solution[_posNode] - state.Solution[_negNode];
            var i = state.Solution[_brNode];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _posNode, _negNode, _cbrNode, _brNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
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
            _cbrNode = ComplexState.Map[ControlBranch];
            _brNode = ComplexState.Map[Branch];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_negNode, _brNode),
                new MatrixLocation(_brNode, _posNode),
                new MatrixLocation(_brNode, _negNode),
                new MatrixLocation(_brNode, _cbrNode));
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
            ComplexElements.Add(1, -1, 1, -1, -BaseParameters.Coefficient);
        }
    }
}
