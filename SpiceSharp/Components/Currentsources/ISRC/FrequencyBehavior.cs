using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected CommonBehaviors.IndependentSourceFrequencyParameters FrequencyParameters { get; private set; }

        /// <summary>
        /// Gets the complex vector elements.
        /// </summary>
        /// <value>
        /// The complex vector elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var v = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            return -v * Complex.Conjugate(FrequencyParameters.Phasor);
        }

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => FrequencyParameters.Phasor;

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

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
            FrequencyParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceFrequencyParameters>();

            // Get matrix elements
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver, null, new[] { PosNode, NegNode });
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
        /// Initializes the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            var value = FrequencyParameters.Phasor;
            ComplexElements.Add(-value, value);
        }
    }
}
