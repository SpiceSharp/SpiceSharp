using System.Numerics;
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
        private readonly ElementSet<Complex> _elements;
        private readonly int _posNode, _negNode, _posPrimeNode;
        private readonly IComplexSimulationState _complex;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v_c"), ParameterName("vd_c"), ParameterInfo("Voltage across the internal diode")]
        public Complex ComplexVoltage => (_complex.Solution[_posPrimeNode] - _complex.Solution[_negNode]) / Parameters.SeriesMultiplier;

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i_c"), ParameterName("id_c"), ParameterInfo("Current through the diode")]
        public Complex ComplexCurrent
        {
            get
            {
                var geq = LocalCapacitance * _complex.Laplace + LocalConductance;
                return ComplexVoltage * geq * Parameters.ParallelMultiplier;
            }
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p_c"), ParameterName("pd_c"), ParameterInfo("Power")]
        public Complex ComplexPower => ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            _complex = context.GetState<IComplexSimulationState>();
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];
            _posPrimeNode = _complex.Map[PosPrime];
            _elements = new ElementSet<Complex>(_complex.Solver,
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
            CalculateCapacitance(LocalVoltage);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var state = _complex;

            var gspr = ModelTemperature.Conductance * Parameters.Area;
            var geq = LocalConductance;
            var xceq = LocalCapacitance * state.Laplace.Imaginary;

            // Load Y-matrix
            var m = Parameters.ParallelMultiplier;
            var n = Parameters.SeriesMultiplier;
            geq *= m / n;
            gspr *= m / n;
            xceq *= m / n;
            _elements.Add(
                gspr, new Complex(geq, xceq), new Complex(geq + gspr, xceq),
                -gspr, -new Complex(geq, xceq), -gspr, -new Complex(geq, xceq));
        }
    }
}
