using System.Numerics;
using SpiceSharp.Entities;
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
        public Complex GetComplexCurrent() => ComplexState.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the power through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var v = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            var i = ComplexState.Solution[BranchEq];
            return -v * Complex.Conjugate(i);
        }

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
            FrequencyParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceFrequencyParameters>();

            // Get matrix elements
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver, new[] {
                new MatrixLocation(PosNode, BranchEq),
                new MatrixLocation(BranchEq, PosNode),
                new MatrixLocation(NegNode, BranchEq),
                new MatrixLocation(BranchEq, NegNode)
            }, new[] { BranchEq });
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
            ComplexElements
                .ThrowIfNotBound(this)
                .Add(1, 1, -1, -1, FrequencyParameters.Phasor);
        }
    }
}
