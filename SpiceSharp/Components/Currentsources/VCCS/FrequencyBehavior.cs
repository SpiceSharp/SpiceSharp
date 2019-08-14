using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// The (pos, ctrlpos) element.
        /// </summary>
        protected MatrixElement<Complex> CPosControlPosPtr { get; private set; }

        /// <summary>
        /// The (pos, ctrlneg) element.
        /// </summary>
        protected MatrixElement<Complex> CPosControlNegPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlpos) element.
        /// </summary>
        protected MatrixElement<Complex> CNegControlPosPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlneg) element.
        /// </summary>
        protected MatrixElement<Complex> CNegControlNegPtr { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetComplexVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("c"), ParameterName("i"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => (_state.Solution[ContPosNode] - _state.Solution[ContNegNode]) * BaseParameters.Coefficient.Value;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public Complex GetComplexPower()
        {
            _state.ThrowIfNotBound(this);
            var v = _state.Solution[PosNode] - _state.Solution[NegNode];
            var i = (_state.Solution[ContPosNode] - _state.Solution[ContNegNode]) * BaseParameters.Coefficient.Value;
            return -v * Complex.Conjugate(i);
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
            CPosControlPosPtr = solver.GetMatrixElement(PosNode, ContPosNode);
            CPosControlNegPtr = solver.GetMatrixElement(PosNode, ContNegNode);
            CNegControlPosPtr = solver.GetMatrixElement(NegNode, ContPosNode);
            CNegControlNegPtr = solver.GetMatrixElement(NegNode, ContNegNode);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var value = BaseParameters.Coefficient.Value;
            CPosControlPosPtr.Value += value;
            CPosControlNegPtr.Value -= value;
            CNegControlPosPtr.Value -= value;
            CNegControlNegPtr.Value += value;
        }
    }
}
