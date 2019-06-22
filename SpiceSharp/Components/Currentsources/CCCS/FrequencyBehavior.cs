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
        /// Device methods and properties 
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }
        [ParameterName("i"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[ControlBranchEq] * BaseParameters.Coefficient.Value;
        }
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = state.Solution[ControlBranchEq] * BaseParameters.Coefficient.Value;
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CPosControlBranchPtr { get; private set; }
        protected MatrixElement<Complex> CNegControlBranchPtr { get; private set; }

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
            CPosControlBranchPtr = solver.GetMatrixElement(PosNode, ControlBranchEq);
            CNegControlBranchPtr = solver.GetMatrixElement(NegNode, ControlBranchEq);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Load the Y-matrix
            CPosControlBranchPtr.Value += BaseParameters.Coefficient.Value;
            CNegControlBranchPtr.Value -= BaseParameters.Coefficient.Value;
        }
    }
}
