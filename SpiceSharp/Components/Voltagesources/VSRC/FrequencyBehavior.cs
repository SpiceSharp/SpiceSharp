using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        protected CommonBehaviors.IndependentSourceFrequencyParameters FrequencyParameters { get; private set; }

        /// <summary>
        /// Gets the (positive, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CPosBranchPtr { get; private set; }
        
        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CNegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchNegPtr { get; private set; }

        /// <summary>
        /// Gets the branch RHS element.
        /// </summary>
        protected VectorElement<Complex> CBranchPtr { get; private set; }

        // Cache
        private ComplexSimulationState _state;

        /// <summary>
        /// Gets the complex voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => FrequencyParameters.Phasor;

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex GetComplexCurrent() => _state.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the power through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetComplexPower()
        {
            _state.ThrowIfNotBound(this);
            var v = _state.Solution[PosNode] - _state.Solution[NegNode];
            var i = _state.Solution[BranchEq];
            return -v * Complex.Conjugate(i);
        }

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

            // Get parameters
            FrequencyParameters = context.GetParameterSet<CommonBehaviors.IndependentSourceFrequencyParameters>();

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver;

            // Get matrix elements
            CPosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            CBranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            CNegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            CBranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);

            // Get rhs elements
            CBranchPtr = solver.GetRhsElement(BranchEq);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            CPosBranchPtr = null;
            CBranchPosPtr = null;
            CNegBranchPtr = null;
            CBranchNegPtr = null;
            CBranchPtr = null;
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
            // Load Y-matrix
            CPosBranchPtr.ThrowIfNotBound(this).Value += 1.0;
            CBranchPosPtr.Value += 1.0;
            CNegBranchPtr.Value -= 1.0;
            CBranchNegPtr.Value -= 1.0;

            // Load Rhs-vector
            CBranchPtr.Value += FrequencyParameters.Phasor;
        }
    }
}
