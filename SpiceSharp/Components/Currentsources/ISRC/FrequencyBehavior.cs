using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected CommonBehaviors.IndependentSourceFrequencyParameters FrequencyParameters { get; private set; }

        /// <summary>
        /// The positive RHS element.
        /// </summary>
        protected VectorElement<Complex> CPosPtr { get; private set; }

        /// <summary>
        /// The negative RHS element.
        /// </summary>
        protected VectorElement<Complex> CNegPtr { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetComplexVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            _state.ThrowIfNotBound(this);
            var v = _state.Solution[PosNode] - _state.Solution[NegNode];
            return -v * Complex.Conjugate(FrequencyParameters.Phasor);
        }

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => FrequencyParameters.Phasor;

        // Cached
        private ComplexSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">Data provider</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            FrequencyParameters = context.GetParameterSet<CommonBehaviors.IndependentSourceFrequencyParameters>();

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver;
            CPosPtr = solver.GetRhsElement(PosNode);
            CNegPtr = solver.GetRhsElement(NegNode);
        }

        /// <summary>
        /// Initializes the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            CPosPtr.Value -= FrequencyParameters.Phasor;
            CNegPtr.Value += FrequencyParameters.Phasor;
        }
    }
}
