using SpiceSharp.Algebra;
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
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
			method.ThrowIfNull(nameof(method));
            _capCharge = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.RealState;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            CalculateCapacitance(vd);
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<double> solver)
        {
            // No extra pointers needed
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            // Calculate the capacitance
            var state = simulation.RealState;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            CalculateCapacitance(vd);

            // Integrate
            _capCharge.Integrate();
            var geq = _capCharge.Jacobian(Capacitance);
            var ceq = _capCharge.RhsCurrent(geq, vd);

            // Store the current
            Current = Current + _capCharge.Derivative;

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
