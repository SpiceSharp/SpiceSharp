using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Capacitor"/>
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<PreciseComplex> PosPosPtr { get; private set; }
        protected MatrixElement<PreciseComplex> NegNegPtr { get; private set; }
        protected MatrixElement<PreciseComplex> PosNegPtr { get; private set; }
        protected MatrixElement<PreciseComplex> NegPosPtr { get; private set; }

        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public PreciseComplex GetVoltage(PreciseComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public PreciseComplex GetCurrent(PreciseComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            var conductance = state.Laplace * Capacitance;
            return (state.Solution[PosNode] - state.Solution[NegNode]) * conductance;
        }
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public PreciseComplex GetPower(PreciseComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            var conductance = state.Laplace * Capacitance;
            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * PreciseComplex.Conjugate(voltage * conductance);
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
            // Not needed
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">The matrix</param>
        public void GetEquationPointers(Solver<PreciseComplex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get matrix pointers
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
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
            var val = state.Laplace * Capacitance;

            // Load the Y-matrix
            PosPosPtr.Value += val;
            NegNegPtr.Value += val;
            PosNegPtr.Value -= val;
            NegPosPtr.Value -= val;
        }
    }
}
