using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = state.Solution[BranchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CPosBranchPtr { get; private set; }
        protected MatrixElement<Complex> CNegBranchPtr { get; private set; }
        protected MatrixElement<Complex> CBranchPosPtr { get; private set; }
        protected MatrixElement<Complex> CBranchNegPtr { get; private set; }
        protected MatrixElement<Complex> CBranchControlPosPtr { get; private set; }
        protected MatrixElement<Complex> CBranchControlNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

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

            CPosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            CNegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            CBranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            CBranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            CBranchControlPosPtr = solver.GetMatrixElement(BranchEq, ContPosNode);
            CBranchControlNegPtr = solver.GetMatrixElement(BranchEq, ContNegNode);
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
            CBranchControlPosPtr.Value -= BaseParameters.Coefficient.Value;
            CBranchControlNegPtr.Value += BaseParameters.Coefficient.Value;
        }
    }
}
