using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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
        protected ElementSet<Complex> ComplexElements { get; private set; }
        
        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v_c"), ParameterName("vd_c"), ParameterInfo("Voltage across the internal diode")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[_posPrimeNode] - ComplexState.Solution[_negNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i_c"), ParameterName("id_c"), ParameterInfo("Current through the diode")]
        public Complex GetComplexCurrent()
        {
            ComplexState.ThrowIfNotBound(this);
            var geq = Capacitance * ComplexState.Laplace + Conductance;
            var voltage = ComplexState.Solution[_posPrimeNode] - ComplexState.Solution[_negNode];
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
            var current = (ComplexState.Solution[_posPrimeNode] - ComplexState.Solution[_negNode]) * geq;
            var voltage = ComplexState.Solution[_posNode] - ComplexState.Solution[_negNode];
            return voltage * -Complex.Conjugate(current);
        }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _posNode, _negNode, _posPrimeNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            ComplexState = context.GetState<IComplexSimulationState>();
            _posNode = ComplexState.Map[context.Nodes[0]];
            _negNode = ComplexState.Map[context.Nodes[1]];
            _posPrimeNode = ComplexState.Map[PosPrime];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_negNode, _negNode),
                new MatrixLocation(_posPrimeNode, _posPrimeNode),
                new MatrixLocation(_posNode, _posPrimeNode),
                new MatrixLocation(_negNode, _posPrimeNode),
                new MatrixLocation(_posPrimeNode, _posNode),
                new MatrixLocation(_posPrimeNode, _negNode));
        }

        /// <summary>
        /// Calculate the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            CalculateCapacitance(Voltage);
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
            ComplexElements.Add(
                gspr, new Complex(geq, xceq), new Complex(geq + gspr, xceq),
                -gspr, -new Complex(geq, xceq), -gspr, -new Complex(geq, xceq));
        }
    }
}
