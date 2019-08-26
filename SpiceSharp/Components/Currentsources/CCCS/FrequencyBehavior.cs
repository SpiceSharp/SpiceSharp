using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Get the voltage. 
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => ComplexState.ThrowIfNotBound(this).Solution[ControlBranchEq] * BaseParameters.Coefficient.Value;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var v = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            var i = ComplexState.Solution[ControlBranchEq] * BaseParameters.Coefficient.Value;
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// The (pos, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CPosControlBranchPtr { get; private set; }

        /// <summary>
        /// the (neg, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CNegControlBranchPtr { get; private set; }

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
        /// Initializes the parameters.
        /// </summary>
        public void InitializeParameters()
        {
        }

        /// <summary>
        /// Bind behavior.
        /// </summary>
        /// <param name="context">Data provider</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.Get<ComplexSimulationState>();
            var solver = ComplexState.Solver;
            CPosControlBranchPtr = solver.GetMatrixElement(PosNode, ControlBranchEq);
            CNegControlBranchPtr = solver.GetMatrixElement(NegNode, ControlBranchEq);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            CPosControlBranchPtr = null;
            CNegControlBranchPtr = null;
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            CPosControlBranchPtr.Value += BaseParameters.Coefficient.Value;
            CNegControlBranchPtr.Value -= BaseParameters.Coefficient.Value;
        }
    }
}
