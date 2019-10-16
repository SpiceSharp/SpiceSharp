using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[_posNode] - ComplexState.Solution[_negNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex GetComplexCurrent()
        {
            ComplexState.ThrowIfNotBound(this);
            var conductance = ComplexState.Laplace * Capacitance;
            return (ComplexState.Solution[_posNode] - ComplexState.Solution[_negNode]) * conductance;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var conductance = ComplexState.Laplace * Capacitance;
            var voltage = ComplexState.Solution[_posNode] - ComplexState.Solution[_negNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _posNode, _negNode;

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
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
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
            ComplexElements.Add(val, -val, -val, val);
        }
    }
}
