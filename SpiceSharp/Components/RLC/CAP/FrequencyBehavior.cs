using System.Numerics;
using SpiceSharp.Algebra;
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
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<Complex> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<Complex> PosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<Complex> NegPosPtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var conductance = state.Laplace * Capacitance;
            return (state.Solution[PosNode] - state.Solution[NegNode]) * conductance;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var conductance = state.Laplace * Capacitance;
            var voltage = state.Solution[PosNode] - state.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

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
        /// <param name="solver">The matrix</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			solver.ThrowIfNull(nameof(solver));

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
			simulation.ThrowIfNull(nameof(simulation));

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
