using System.Numerics;
using SpiceSharp.Circuits;
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
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ComplexMatrixElementSet ComplexMatrixElements { get; private set; }
        
        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v_c"), ParameterName("vd_c"), ParameterInfo("Voltage across the internal diode")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosPrimeNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i_c"), ParameterName("id_c"), ParameterInfo("Current through the diode")]
        public Complex GetComplexCurrent()
        {
            ComplexState.ThrowIfNotBound(this);
            var geq = Capacitance * ComplexState.Laplace + Conductance;
            var voltage = ComplexState.Solution[PosPrimeNode] - ComplexState.Solution[NegNode];
            return voltage * geq;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p_c"), ParameterName("pd_c"), ParameterInfo("Power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var geq = Capacitance * ComplexState.Laplace + Conductance;
            var current = (ComplexState.Solution[PosPrimeNode] - ComplexState.Solution[NegNode]) * geq;
            var voltage = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            return voltage * -Complex.Conjugate(current);
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

            ComplexMatrixElements = new ComplexMatrixElementSet(ComplexState.Solver,
                new MatrixPin(PosNode, PosNode),
                new MatrixPin(NegNode, NegNode),
                new MatrixPin(PosPrimeNode, PosPrimeNode),
                new MatrixPin(PosNode, PosPrimeNode),
                new MatrixPin(NegNode, PosPrimeNode),
                new MatrixPin(PosPrimeNode, PosNode),
                new MatrixPin(PosPrimeNode, NegNode));
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
        /// Calculate the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vd = BiasingState.Solution[PosPrimeNode] - BiasingState.Solution[NegNode];
            CalculateCapacitance(vd);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var state = ComplexState;

            var gspr = ModelTemperature.Conductance * BaseParameters.Area;
            var geq = Conductance;
            var xceq = Capacitance * state.Laplace.Imaginary;

            // Load Y-matrix
            ComplexMatrixElements.Add(
                gspr, new Complex(geq, xceq), new Complex(geq + gspr, xceq),
                -gspr, -new Complex(geq, xceq), -gspr, -new Complex(geq, xceq));
        }
    }
}
