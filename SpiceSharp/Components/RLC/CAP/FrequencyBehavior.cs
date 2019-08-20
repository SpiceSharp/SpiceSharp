using System.Numerics;
using SpiceSharp.Algebra;
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
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<Complex> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<Complex> PosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<Complex> NegPosPtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex GetComplexVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex GetComplexCurrent()
        {
            _state.ThrowIfNotBound(this);
            var conductance = _state.Laplace * Capacitance;
            return (_state.Solution[PosNode] - _state.Solution[NegNode]) * conductance;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex GetComplexPower()
        {
            _state.ThrowIfNotBound(this);
            var conductance = _state.Laplace * Capacitance;
            var voltage = _state.Solution[PosNode] - _state.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

        // Cache
        private ComplexSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver;
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
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
            var val = _state.Laplace * Capacitance;

            // Load the Y-matrix
            PosPosPtr.Value += val;
            NegNegPtr.Value += val;
            PosNegPtr.Value -= val;
            NegPosPtr.Value -= val;
        }
    }
}
