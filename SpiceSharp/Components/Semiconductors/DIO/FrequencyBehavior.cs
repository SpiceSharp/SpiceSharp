using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Diode"/>
    /// </summary>
    public class FrequencyBehavior : DynamicParameterBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CPosPosPrimePtr { get; private set; }
        protected MatrixElement<Complex> CNegPosPrimePtr { get; private set; }
        protected MatrixElement<Complex> CPosPrimePosPtr { get; private set; }
        protected MatrixElement<Complex> CPosPrimeNegPtr { get; private set; }
        protected MatrixElement<Complex> CPosPosPtr { get; private set; }
        protected MatrixElement<Complex> CNegNegPtr { get; private set; }
        protected MatrixElement<Complex> CPosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("vd"), ParameterInfo("Voltage across the internal diode")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosPrimeNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Current through the diode")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var geq = Capacitance * state.Laplace + Conductance;
            var voltage = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            return voltage * geq;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var geq = Capacitance * state.Laplace + Conductance;
            var current = (state.Solution[PosPrimeNode] - state.Solution[NegNode]) * geq;
            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * -Complex.Conjugate(current);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			solver.ThrowIfNull(nameof(solver));

            // Get matrix pointers
            CPosPosPrimePtr = solver.GetMatrixElement(PosNode, PosPrimeNode);
            CNegPosPrimePtr = solver.GetMatrixElement(NegNode, PosPrimeNode);
            CPosPrimePosPtr = solver.GetMatrixElement(PosPrimeNode, PosNode);
            CPosPrimeNegPtr = solver.GetMatrixElement(PosPrimeNode, NegNode);
            CPosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            CNegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            CPosPrimePosPrimePtr = solver.GetMatrixElement(PosPrimeNode, PosPrimeNode);
        }
        
        /// <summary>
        /// Calculate AC parameters
        /// </summary>
        /// <param name="simulation"></param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));
            var state = simulation.RealState;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            CalculateCapacitance(vd);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.ComplexState;

            var gspr = ModelTemperature.Conductance * BaseParameters.Area;
            var geq = Conductance;
            var xceq = Capacitance * state.Laplace.Imaginary;

            // Load Y-matrix
            CPosPosPtr.Value += gspr;
            CNegNegPtr.Value += new Complex(geq, xceq);
            CPosPrimePosPrimePtr.Value += new Complex(geq + gspr, xceq);
            CPosPosPrimePtr.Value -= gspr;
            CNegPosPrimePtr.Value -= new Complex(geq, xceq);
            CPosPrimePosPtr.Value -= gspr;
            CPosPrimeNegPtr.Value -= new Complex(geq, xceq);
        }
    }
}
