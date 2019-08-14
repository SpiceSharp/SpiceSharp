using System;
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

        /// <summary>
        /// Gets the complex voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => FrequencyParameters.Phasor;

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));
            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = state.Solution[BranchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            FrequencyParameters = provider.GetParameterSet<CommonBehaviors.IndependentSourceFrequencyParameters>();
        }
        
        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			solver.ThrowIfNull(nameof(solver));

            // Get matrix elements
            CPosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            CBranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            CNegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            CBranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);

            // Get rhs elements
            CBranchPtr = solver.GetRhsElement(BranchEq);
        }
        
        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            // Load Y-matrix
            CPosBranchPtr.Value += 1.0;
            CBranchPosPtr.Value += 1.0;
            CNegBranchPtr.Value -= 1.0;
            CBranchNegPtr.Value -= 1.0;

            // Load Rhs-vector
            CBranchPtr.Value += FrequencyParameters.Phasor;
        }
    }
}
