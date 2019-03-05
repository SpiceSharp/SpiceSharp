using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Numerics;
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
        /// <value>
        /// The frequency parameters.
        /// </value>
        protected CommonBehaviors.IndependentFrequencyParameters FrequencyParameters { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement<PreciseComplex> CPosBranchPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CNegBranchPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBranchPosPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBranchNegPtr { get; private set; }
        protected VectorElement<PreciseComplex> CBranchPtr { get; private set; }

        /// <summary>
        /// Gets the complex voltage applied by the source.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public PreciseComplex ComplexVoltage => FrequencyParameters.Phasor;

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public PreciseComplex GetCurrent(PreciseComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public PreciseComplex GetPower(PreciseComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = state.Solution[BranchEq];
            return -v * PreciseComplex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            FrequencyParameters = provider.GetParameterSet<CommonBehaviors.IndependentFrequencyParameters>();
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
        public void GetEquationPointers(Solver<PreciseComplex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

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
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

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
