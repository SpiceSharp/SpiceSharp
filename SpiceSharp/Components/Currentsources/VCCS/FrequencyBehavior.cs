using System;
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
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CPosControlPosPtr { get; private set; }
        protected MatrixElement<Complex> CPosControlNegPtr { get; private set; }
        protected MatrixElement<Complex> CNegControlPosPtr { get; private set; }
        protected MatrixElement<Complex> CNegControlNegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }
        [ParameterName("c"), ParameterName("i"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[ContPosNode] - state.Solution[ContNegNode]) * BaseParameters.Coefficient.Value;
        }
        [ParameterName("p"), ParameterInfo("Power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = (state.Solution[ContPosNode] - state.Solution[ContNegNode]) * BaseParameters.Coefficient.Value;
            return -v * Complex.Conjugate(i);
        }

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

            // Get matrix pointers
            CPosControlPosPtr = solver.GetMatrixElement(PosNode, ContPosNode);
            CPosControlNegPtr = solver.GetMatrixElement(PosNode, ContNegNode);
            CNegControlPosPtr = solver.GetMatrixElement(NegNode, ContPosNode);
            CNegControlNegPtr = solver.GetMatrixElement(NegNode, ContNegNode);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
            var value = BaseParameters.Coefficient.Value;
            CPosControlPosPtr.Value += value;
            CPosControlNegPtr.Value -= value;
            CNegControlPosPtr.Value -= value;
            CNegControlNegPtr.Value += value;
        }
    }
}
