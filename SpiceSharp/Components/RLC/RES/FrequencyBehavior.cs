using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Resistor"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (complex) voltage across the resistor.
        /// </summary>
        [ParameterName("v_c"), ParameterInfo("Complex voltage across the capacitor.")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Gets the (complex) current through the resistor.
        /// </summary>
        [ParameterName("i_c"), ParameterInfo("Complex current through the capacitor.")]
        public Complex GetComplexCurrent()
        {
            ComplexState.ThrowIfNotBound(this);
            var voltage = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            return voltage * Conductance;
        }

        /// <summary>
        /// Gets the (complex) power dissipated by the resistor.
        /// </summary>
        [ParameterName("p_c"), ParameterInfo("Power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var voltage = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage) * Conductance;
        }

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

            ComplexState = context.States.GetValue<IComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(PosNode, PosNode),
                new MatrixLocation(PosNode, NegNode),
                new MatrixLocation(NegNode, PosNode),
                new MatrixLocation(NegNode, NegNode)
                );
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
        /// Initialize the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            ComplexElements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
