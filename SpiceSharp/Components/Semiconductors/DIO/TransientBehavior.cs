using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
    /// </summary>
    public class TransientBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the capacitance charge.
        /// </summary>
        [ParameterName("charge"), ParameterInfo("Diode capacitor charge")]
        public sealed override double CapCharge
        {
            get => _capCharge.Current;
            protected set => _capCharge.Current = value;
        }

        /// <summary>
        /// Gets the capacitor current.
        /// </summary>
        [ParameterName("capcur"), ParameterInfo("Diode capacitor current")]
        public double CapCurrent => _capCharge.Derivative;

        /// <summary>
        /// The charge on the junction capacitance
        /// </summary>
        private StateDerivative _capCharge;

        /// <summary>
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            var method = ((TimeSimulation)simulation).Method;
            _capCharge = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            var state = State;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            CalculateCapacitance(vd);
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        void ITimeBehavior.Load()
        {
            // Calculate the capacitance
            var state = State;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            CalculateCapacitance(vd);

            // Integrate
            _capCharge.Integrate();
            var geq = _capCharge.Jacobian(Capacitance);
            var ceq = _capCharge.RhsCurrent(geq, vd);

            // Store the current
            Current += _capCharge.Derivative;

            // Load Rhs vector
            NegPtr.Value += ceq;
            PosPrimePtr.Value -= ceq;

            // Load Y-matrix
            NegNegPtr.Value += geq;
            PosPrimePosPrimePtr.Value += geq;
            NegPosPrimePtr.Value -= geq;
            PosPrimeNegPtr.Value -= geq;
        }
    }
}
