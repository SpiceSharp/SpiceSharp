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
        public Complex GetComplexVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => _state.ThrowIfNotBound(this).Solution[ControlBranchEq] * BaseParameters.Coefficient.Value;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            _state.ThrowIfNotBound(this);
            var v = _state.Solution[PosNode] - _state.Solution[NegNode];
            var i = _state.Solution[ControlBranchEq] * BaseParameters.Coefficient.Value;
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

        // Cache
        private ComplexSimulationState _state;

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
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver;
            CPosControlBranchPtr = solver.GetMatrixElement(PosNode, ControlBranchEq);
            CNegControlBranchPtr = solver.GetMatrixElement(NegNode, ControlBranchEq);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
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
