using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Resistor"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (complex) voltage across the resistor.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage across the capacitor.")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the (complex) current through the resistor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Complex current through the capacitor.")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * Conductance;
        }

        /// <summary>
        /// Gets the (complex) power dissipated by the resistor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));
            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage) * Conductance;
        }

        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CNegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CPosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CNegPosPtr { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
            // Not needed
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
            solver.ThrowIfNull(nameof(solver));

            // Get matrix pointers
            CPosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            CNegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            CPosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
            CNegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
            var conductance = Conductance;
            CPosPosPtr.Value += conductance;
            CNegNegPtr.Value += conductance;
            CPosNegPtr.Value -= conductance;
            CNegPosPtr.Value -= conductance;
        }
    }
}
