using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Capacitor"/>.
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ComplexOnePortElementSet ComplexMatrixElements { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex GetComplexCurrent()
        {
            ComplexState.ThrowIfNotBound(this);
            var conductance = ComplexState.Laplace * Capacitance;
            return (ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode]) * conductance;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var conductance = ComplexState.Laplace * Capacitance;
            var voltage = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; private set; }

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

            ComplexState = context.States.GetValue<ComplexSimulationState>();
            ComplexMatrixElements = new ComplexOnePortElementSet(ComplexState.Solver, PosNode, NegNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexMatrixElements?.Destroy();
            ComplexMatrixElements = null;
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            // Not needed
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var val = ComplexState.Laplace * Capacitance;
            ComplexMatrixElements.AddOnePort(val);
        }
    }
}
