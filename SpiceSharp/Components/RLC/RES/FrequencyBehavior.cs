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
    public class FrequencyBehavior : LoadBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (complex) voltage across the capacitor.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Complex voltage across the capacitor.")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the (complex) current.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterInfo("Complex current through the capacitor.")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * Conductance;
        }

        /// <summary>
        /// Gets the (complex) power dissipated by the capacitor.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage) * Conductance;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CPosPosPtr { get; private set; }
        protected MatrixElement<Complex> CNegNegPtr { get; private set; }
        protected MatrixElement<Complex> CPosNegPtr { get; private set; }
        protected MatrixElement<Complex> CNegPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        /// <exception cref="NotImplementedException"></exception>
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
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

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
