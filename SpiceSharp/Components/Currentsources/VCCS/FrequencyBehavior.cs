using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
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
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("c"), ParameterName("i"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => (ComplexState.Solution[ContPosNode] - ComplexState.Solution[ContNegNode]) * BaseParameters.Coefficient.Value;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var v = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            var i = (ComplexState.Solution[ContPosNode] - ComplexState.Solution[ContNegNode]) * BaseParameters.Coefficient.Value;
            return -v * Complex.Conjugate(i);
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
            ComplexState = context.States.Get<ComplexSimulationState>();
            var solver = ComplexState.Solver;
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
