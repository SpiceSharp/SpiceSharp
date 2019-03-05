using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Numerics;
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
        protected MatrixElement<PreciseComplex> CPosPosPrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CNegPosPrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CPosPrimePosPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CPosPrimeNegPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CPosPosPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CNegNegPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CPosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("vd"), ParameterInfo("Voltage across the internal diode")]
        public PreciseComplex GetVoltage(PreciseComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[PosPrimeNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Current through the diode")]
        public PreciseComplex GetCurrent(PreciseComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
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
        public PreciseComplex GetPower(PreciseComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            var geq = Capacitance * state.Laplace + Conductance;
            var current = (state.Solution[PosPrimeNode] - state.Solution[NegNode]) * geq;
            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * -PreciseComplex.Conjugate(current);
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
        public void GetEquationPointers(Solver<PreciseComplex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

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
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));
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
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;

            var gspr = ModelTemperature.Conductance * BaseParameters.Area;
            var geq = Conductance;
            var xceq = Capacitance * (double)state.Laplace.Imaginary;

            // Load Y-matrix
            CPosPosPtr.Value += gspr;
            CNegNegPtr.Value += new PreciseComplex(geq, xceq);
            CPosPrimePosPrimePtr.Value += new PreciseComplex(geq + gspr, xceq);
            CPosPosPrimePtr.Value -= gspr;
            CNegPosPrimePtr.Value -= new PreciseComplex(geq, xceq);
            CPosPrimePosPtr.Value -= gspr;
            CPosPrimeNegPtr.Value -= new PreciseComplex(geq, xceq);
        }
    }
}
