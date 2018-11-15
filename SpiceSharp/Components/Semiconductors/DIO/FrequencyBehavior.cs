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
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
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
        /// Gets the junction capacitance
        /// </summary>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance { get; protected set; }
        [ParameterName("vd"), ParameterInfo("Voltage across the internal diode")]
        public Complex GetDiodeVoltage(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[PosPrimeNode] - state.Solution[NegNode];
        }
        [ParameterName("v"), ParameterInfo("Voltage across the diode")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Current through the diode")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var geq = Capacitance * state.Laplace + Conduct;
            var voltage = state.Solution[PosPrimeNode] - state.Solution[NegNode];
            return voltage * geq;
        }
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var geq = Capacitance * state.Laplace + Conduct;
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
            double capd;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];

            // charge storage elements
            var czero = TempJunctionCap * BaseParameters.Area;
            if (vd < TempDepletionCap)
            {
                var arg = 1 - vd / ModelParameters.JunctionPotential;
                var sarg = Math.Exp(-ModelParameters.GradingCoefficient * Math.Log(arg));
                capd = ModelParameters.TransitTime * Conduct + czero * sarg;
            }
            else
            {
                var czof2 = czero / ModelTemperature.F2;
                capd = ModelParameters.TransitTime * Conduct + czof2 * (ModelTemperature.F3 + ModelParameters.GradingCoefficient * vd / ModelParameters.JunctionPotential);
            }
            Capacitance = capd;
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
            var geq = Conduct;
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
