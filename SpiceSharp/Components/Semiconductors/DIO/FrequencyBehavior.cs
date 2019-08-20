using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Diode"/>
    /// </summary>
    public class FrequencyBehavior : DynamicParameterBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (external positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CNegPosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (positive, external positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPrimePosPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPrimeNegPtr { get; private set; }

        /// <summary>
        /// Gets the external (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CNegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("vd"), ParameterInfo("Voltage across the internal diode")]
        public Complex GetComplexVoltage() => _state.ThrowIfNotBound(this).Solution[PosPrimeNode] - _state.Solution[NegNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Current through the diode")]
        public Complex GetComplexCurrent()
        {
            _state.ThrowIfNotBound(this);
            var geq = Capacitance * _state.Laplace + Conductance;
            var voltage = _state.Solution[PosPrimeNode] - _state.Solution[NegNode];
            return voltage * geq;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public Complex GetComplexPower()
        {
            _state.ThrowIfNotBound(this);
            var geq = Capacitance * _state.Laplace + Conductance;
            var current = (_state.Solution[PosPrimeNode] - _state.Solution[NegNode]) * geq;
            var voltage = _state.Solution[PosNode] - _state.Solution[NegNode];
            return voltage * -Complex.Conjugate(current);
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
            CPosPosPrimePtr = solver.GetMatrixElement(PosNode, PosPrimeNode);
            CNegPosPrimePtr = solver.GetMatrixElement(NegNode, PosPrimeNode);
            CPosPrimePosPtr = solver.GetMatrixElement(PosPrimeNode, PosNode);
            CPosPrimeNegPtr = solver.GetMatrixElement(PosPrimeNode, NegNode);
            CPosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            CNegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            CPosPrimePosPrimePtr = solver.GetMatrixElement(PosPrimeNode, PosPrimeNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            CPosPosPrimePtr = null;
            CNegPosPrimePtr = null;
            CPosPrimePosPtr = null;
            CPosPrimeNegPtr = null;
            CPosPosPtr = null;
            CNegNegPtr = null;
            CPosPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Calculate the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vd = State.Solution[PosPrimeNode] - State.Solution[NegNode];
            CalculateCapacitance(vd);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var state = _state;

            var gspr = ModelTemperature.Conductance * BaseParameters.Area;
            var geq = Conductance;
            var xceq = Capacitance * state.Laplace.Imaginary;

            // Load Y-matrix
            CPosPosPtr.Value += gspr;
            CNegNegPtr.Value += new Complex(geq, xceq);
            CPosPrimePosPrimePtr.Value += new Complex(geq + gspr, xceq);
            CPosPosPrimePtr.Value -= gspr;
            CNegPosPrimePtr.Value -= new Complex(geq, xceq);
            CPosPrimePosPtr.Value -= gspr;
            CPosPrimeNegPtr.Value -= new Complex(geq, xceq);
        }
    }
}
